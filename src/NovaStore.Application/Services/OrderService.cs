using Microsoft.EntityFrameworkCore;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.Domain.Data;
using NovaStore.Domain.Models;
using NovaStore.Domain.Models.Enums;

namespace NovaStore.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly NovaStoreDbContext _context;

        public OrderService(NovaStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto> CreateFromCartAsync(int userId, CreateOrderDto dto)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            var user = await _context.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // Validate shipping address belongs to user
            if (dto.ShippingAddressId.HasValue)
            {
                var address = await _context.Addresses.FindAsync(dto.ShippingAddressId.Value);
                if (address == null || address.UserId != userId)
                    throw new InvalidOperationException("Shipping address not found or does not belong to the current user.");
            }

            // Validate stock
            foreach (var item in cartItems)
            {
                if (item.Product == null || !item.Product.IsActive)
                    throw new InvalidOperationException($"Product '{item.Product?.Name ?? "Unknown"}' is not available.");
                if (item.Product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Not enough stock for '{item.Product.Name}'.");
            }

            decimal subtotal = cartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
            decimal discountAmount = 0;

            // Promo code handling
            if (!string.IsNullOrWhiteSpace(dto.PromoCode))
            {
                switch (dto.PromoCode.ToUpper())
                {
                    case "WELCOME10":
                        discountAmount = subtotal * 0.10m;
                        break;
                    case "SAVE50":
                        discountAmount = 500m;
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid promo code '{dto.PromoCode}'.");
                }
            }

            decimal totalAmount = subtotal - discountAmount;
            if (totalAmount < 0) totalAmount = 0;

            var order = new Order
            {
                UserId = userId,
                CustomerName = user.FullName ?? user.Username,
                ShippingAddressId = dto.ShippingAddressId,
                ShippingNotes = dto.ShippingNotes,
                PaymentMethod = dto.PaymentMethod,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount > 0 ? discountAmount : null,
                PromoCode = dto.PromoCode,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Created,
                PaymentStatus = PaymentStatus.Pending,
                ShippingStatus = ShippingStatus.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product!.Name,
                    ProductImageUrl = cartItem.Product.ImageUrl,
                    UnitPrice = cartItem.Product.Price,
                    Quantity = cartItem.Quantity
                };
                _context.OrderItems.Add(orderItem);

                // Decrease stock
                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return MapToDto(order);
        }

        public async Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return null;

            if (dto.Status != null) order.Status = Enum.Parse<OrderStatus>(dto.Status);
            if (dto.PaymentStatus != null) order.PaymentStatus = Enum.Parse<PaymentStatus>(dto.PaymentStatus);
            if (dto.ShippingStatus != null) order.ShippingStatus = Enum.Parse<ShippingStatus>(dto.ShippingStatus);
            if (dto.ShippedAt.HasValue) order.ShippedAt = dto.ShippedAt;
            if (dto.DeliveredAt.HasValue) order.DeliveredAt = dto.DeliveredAt;

            if (dto.Status == "Shipped" && order.ShippedAt == null)
                order.ShippedAt = DateTime.UtcNow;
            if (dto.Status == "Delivered" && order.DeliveredAt == null)
                order.DeliveredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.CustomerName,
                ShippingAddressId = order.ShippingAddressId,
                ShippingNotes = order.ShippingNotes,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus.ToString(),
                ShippingStatus = order.ShippingStatus.ToString(),
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                PromoCode = order.PromoCode,
                OrderDate = order.OrderDate,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                Status = order.Status.ToString(),
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductImageUrl = oi.ProductImageUrl,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    Subtotal = oi.Subtotal
                }).ToList()
            };
        }
    }
}
