using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.Infrastructure.Data;
using NovaStore.Domain.Models;

namespace NovaStore.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly NovaStoreDbContext _context;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

        public ProductService(NovaStoreDbContext context, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();
            query = query.AsNoTracking();

            // Filter by active only if not specified
            if (searchDto.IsActive == null)
                query = query.Where(p => p.IsActive);
            else
                query = query.Where(p => p.IsActive == searchDto.IsActive);

            // Search term (using ILike for PostgreSQL index-friendly search)
            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{searchDto.SearchTerm}%"));
            }

            // Category filter
            if (searchDto.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == searchDto.CategoryId.Value);

            // Price range
            if (searchDto.MinPrice.HasValue)
                query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
            if (searchDto.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);

            // Brand filter (using ILike for case-insensitive comparison)
            if (!string.IsNullOrWhiteSpace(searchDto.Brand))
                query = query.Where(p => p.Brand != null && EF.Functions.ILike(p.Brand, searchDto.Brand));

            // Sorting
            query = (searchDto.SortBy?.ToLower()) switch
            {
                "price" => searchDto.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "name" => searchDto.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "rating" => query.OrderByDescending(p => p.AverageRating ?? 0),
                "created" => searchDto.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return new PagedResult<ProductDto>
            {
                Items = products.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            };
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var cacheKey = $"product:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                return JsonSerializer.Deserialize<ProductDto>(cached);
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            var dto = MapToDto(product);

            // Cache for 10 minutes
            var cacheOptions = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), cacheOptions);

            return dto;
        }

        public async Task<PagedResult<ProductDto>> GetByCategoryAsync(int categoryId, int page = 1, int pageSize = 20)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ProductDto>
            {
                Items = products.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {dto.CategoryId} not found.");

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity ?? 0,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.ImageUrl,
                Brand = dto.Brand,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"product:{product.Id}");

            return MapToDto(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            if (dto.Name != null) product.Name = dto.Name;
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;
            if (dto.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category == null)
                    throw new KeyNotFoundException($"Category with ID {dto.CategoryId} not found.");
                product.CategoryId = dto.CategoryId.Value;
            }
            if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
            if (dto.Brand != null) product.Brand = dto.Brand;
            if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"product:{id}");

            return MapToDto(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"product:{id}");
            return true;
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                ImageUrl = product.ImageUrl,
                Brand = product.Brand,
                IsActive = product.IsActive,
                AverageRating = product.AverageRating,
                ReviewsCount = product.ReviewsCount,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
