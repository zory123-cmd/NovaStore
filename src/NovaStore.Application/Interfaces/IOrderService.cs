using NovaStore.Application.DTOs;

namespace NovaStore.Application.Interfaces
{
    public interface IOrderService
    {
        Task<PagedResult<OrderDto>> GetAllAsync(int page = 1, int pageSize = 20);
        Task<PagedResult<OrderDto>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 20);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto> CreateFromCartAsync(int userId, CreateOrderDto dto);
        Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
