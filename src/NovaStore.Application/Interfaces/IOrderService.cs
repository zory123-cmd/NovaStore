using NovaStore.Application.DTOs;

namespace NovaStore.Application.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllAsync();
        Task<List<OrderDto>> GetUserOrdersAsync(int userId);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto> CreateFromCartAsync(int userId, CreateOrderDto dto);
        Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
