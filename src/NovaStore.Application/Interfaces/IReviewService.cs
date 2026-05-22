using NovaStore.Application.DTOs;

namespace NovaStore.Application.Interfaces
{
    public interface IReviewService
    {
        Task<PagedResult<ReviewDto>> GetByProductAsync(int productId, int page = 1, int pageSize = 20);
        Task<ReviewDto> CreateAsync(int userId, CreateReviewDto dto);
        Task<ReviewDto?> UpdateAsync(int userId, int id, CreateReviewDto dto);
        Task<bool> DeleteAsync(int userId, int id);
    }
}
