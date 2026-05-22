using Microsoft.EntityFrameworkCore;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.Domain.Data;
using NovaStore.Domain.Models;

namespace NovaStore.Application.Services
{
    public class CartService : ICartService
    {
        private readonly NovaStoreDbContext _context;

        public CartService(NovaStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<CartItemDto>> GetUserCartAsync(int userId)
        {
            var items = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync();

            return items.Select(MapToDto).ToList();
        }

        public async Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");
            if (!product.IsActive)
                throw new InvalidOperationException("Product is not available.");
            if (product.StockQuantity < dto.Quantity)
                throw new InvalidOperationException("Not enough stock available.");

            var existingItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + dto.Quantity;
                if (newQuantity > product.StockQuantity)
                    throw new InvalidOperationException("Not enough stock.");
                existingItem.Quantity = newQuantity;
                await _context.SaveChangesAsync();
                return MapToDto(existingItem);
            }

            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            // Reload with product data
            await _context.Entry(cartItem).Reference(ci => ci.Product).LoadAsync();
            return MapToDto(cartItem);
        }

        public async Task<bool> UpdateQuantityAsync(int userId, int cartItemId, UpdateCartItemDto dto)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (item == null)
                return false;

            if (dto.Quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                if (item.Product != null && item.Product.StockQuantity < dto.Quantity)
                    throw new InvalidOperationException("Not enough stock available.");
                item.Quantity = dto.Quantity;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (item == null)
                return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(int userId)
        {
            var items = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Product != null ? ci.Product.Price * ci.Quantity : 0);
        }

        private static CartItemDto MapToDto(CartItem item)
        {
            return new CartItemDto
            {
                Id = item.Id,
                UserId = item.UserId,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name,
                ProductImageUrl = item.Product?.ImageUrl,
                UnitPrice = item.Product?.Price ?? 0,
                Quantity = item.Quantity,
                Subtotal = (item.Product?.Price ?? 0) * item.Quantity,
                CreatedAt = item.CreatedAt
            };
        }
    }
}
