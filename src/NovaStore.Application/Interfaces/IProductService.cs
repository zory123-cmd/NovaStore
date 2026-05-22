using NovaStore.Application.DTOs;

namespace NovaStore.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<PagedResult<ProductDto>> GetByCategoryAsync(int categoryId, int page = 1, int pageSize = 20);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
