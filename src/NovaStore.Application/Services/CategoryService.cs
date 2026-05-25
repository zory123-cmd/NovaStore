using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.Infrastructure.Data;
using NovaStore.Domain.Models;

namespace NovaStore.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly NovaStoreDbContext _context;
        private readonly IDistributedCache _cache;

        public CategoryService(NovaStoreDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            const string cacheKey = "categories:all";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<List<CategoryDto>>(cached) ?? new List<CategoryDto>();

            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var result = categories.Select(c => MapToDto(c)).ToList();

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

            return result;
        }

        public async Task<List<CategoryDto>> GetRootAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToDto).ToList();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            return category == null ? null : MapToDto(category);
        }

        public async Task<List<CategoryDto>> GetSubcategoriesAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found.");

            return category.SubCategories.Select(MapToDto).ToList();
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            if (dto.ParentCategoryId.HasValue)
            {
                var parent = await _context.Categories.FindAsync(dto.ParentCategoryId.Value);
                if (parent == null)
                    throw new KeyNotFoundException($"Parent category with ID {dto.ParentCategoryId} not found.");
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ParentCategoryId = dto.ParentCategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync("categories:all");

            return MapToDto(category);
        }

        public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return null;

            if (dto.Name != null) category.Name = dto.Name;
            if (dto.Description != null) category.Description = dto.Description;
            if (dto.ImageUrl != null) category.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();

            await _cache.RemoveAsync("categories:all");

            return MapToDto(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            if (category.SubCategories.Any())
                throw new InvalidOperationException("Cannot delete category with subcategories.");

            if (await _context.Products.AnyAsync(p => p.CategoryId == id))
                throw new InvalidOperationException("Cannot delete category with associated products.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync("categories:all");
            return true;
        }

        private static CategoryDto MapToDto(Category category, int depth = 0)
        {
            const int maxDepth = 3;
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                CreatedAt = category.CreatedAt,
                SubCategories = depth < maxDepth
                    ? category.SubCategories?.Select(c => MapToDto(c, depth + 1)).ToList() ?? new()
                    : new List<CategoryDto>()
            };
        }
    }
}
