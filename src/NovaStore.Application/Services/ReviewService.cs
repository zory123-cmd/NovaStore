using Microsoft.EntityFrameworkCore;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.Domain.Data;
using NovaStore.Domain.Models;

namespace NovaStore.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly NovaStoreDbContext _context;

        public ReviewService(NovaStoreDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ReviewDto>> GetByProductAsync(int productId, int page = 1, int pageSize = 20)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ReviewDto>
            {
                Items = reviews.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ReviewDto> CreateAsync(int userId, CreateReviewDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");

            var existing = await _context.Reviews
                .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

            if (existing)
                throw new InvalidOperationException("You have already reviewed this product.");

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            await UpdateProductRating(dto.ProductId);

            await _context.Entry(review).Reference(r => r.User).LoadAsync();
            return MapToDto(review);
        }

        public async Task<ReviewDto?> UpdateAsync(int userId, int id, CreateReviewDto dto)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
                return null;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await UpdateProductRating(review.ProductId);

            return MapToDto(review);
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
                return false;

            var productId = review.ProductId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            await UpdateProductRating(productId);
            return true;
        }

        private async Task UpdateProductRating(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            var avg = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double?)r.Rating);

            product.AverageRating = avg;
            product.ReviewsCount = await _context.Reviews
                .CountAsync(r => r.ProductId == productId);

            await _context.SaveChangesAsync();
        }

        private static ReviewDto MapToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = review.User?.Username,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }
    }
}
