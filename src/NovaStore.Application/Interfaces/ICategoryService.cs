using NovaStore.Application.DTOs;

namespace NovaStore.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<List<CategoryDto>> GetRootAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<List<CategoryDto>> GetSubcategoriesAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
        Task<CategoryDto?> UpdateAsync(int id, string? name, string? description, string? imageUrl);
        Task<bool> DeleteAsync(int id);
    }
}
