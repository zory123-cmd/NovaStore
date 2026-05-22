using NovaStore.Application.DTOs;

namespace NovaStore.Application.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemDto>> GetUserCartAsync(int userId);
        Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto dto);
        Task<bool> UpdateQuantityAsync(int userId, int cartItemId, UpdateCartItemDto dto);
        Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
    }
}
