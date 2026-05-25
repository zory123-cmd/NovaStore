using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public OrderService(NovaStoreDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<PagedResult<OrderDto>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<OrderDto>
            {
                Items = orders.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<OrderDto>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<OrderDto>
            {
                Items = orders.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
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

            // Begin transaction for atomicity
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Validate stock with atomic decrement (race condition protection)
            foreach (var item in cartItems)
            {
                if (item.Product == null || !item.Product.IsActive)
                    throw new InvalidOperationException($"Product '{item.Product?.Name ?? "Unknown"}' is not available.");
                if (item.Product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Not enough stock for '{item.Product.Name}'.");

                // Atomic stock decrement via raw SQL to prevent overselling
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Products\" SET \"StockQuantity\" = \"StockQuantity\" - {0}, \"UpdatedAt\" = NOW() WHERE \"Id\" = {1} AND \"StockQuantity\" >= {0}",
                    item.Quantity, item.ProductId);

                if (rowsAffected == 0)
                    throw new InvalidOperationException($"Not enough stock for '{item.Product.Name}'. Please refresh your cart.");
            }

            decimal subtotal = cartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
            decimal discountAmount = 0;

            // Promo code handling (configurable via appsettings.json)
            if (!string.IsNullOrWhiteSpace(dto.PromoCode))
            {
                var promoCode = dto.PromoCode.ToUpper();
                var promoConfig = _configuration.GetSection($"PromoCodes:{promoCode}");
                if (!promoConfig.Exists())
                    throw new InvalidOperationException($"Invalid promo code '{dto.PromoCode}'.");

                var discountType = promoConfig["Type"];
                var discountValue = decimal.TryParse(promoConfig["Value"] ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue)
                    ? parsedValue
                    : 0m;

                discountAmount = discountType?.ToUpper() switch
                {
                    "PERCENTAGE" => subtotal * discountValue / 100m,
                    "FIXED" => discountValue,
                    _ => throw new InvalidOperationException($"Invalid promo code configuration for '{dto.PromoCode}'.")
                };
            }

            decimal totalAmount = subtotal - discountAmount;
            if (totalAmount < 0) totalAmount = 0;

            const decimal minOrderAmount = 1m;
            if (totalAmount < minOrderAmount)
                throw new InvalidOperationException($"Minimum order amount is {minOrderAmount:C}.");

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

            // Create order items via navigation collection (EF Core auto-resolves FK)
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product!.Name,
                    ProductImageUrl = cartItem.Product.ImageUrl,
                    UnitPrice = cartItem.Product.Price,
                    Quantity = cartItem.Quantity
                };
                order.OrderItems.Add(orderItem);
            }

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);

            // Single SaveChanges within transaction
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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

            OrderStatus? parsedStatus = null;
            PaymentStatus? parsedPaymentStatus = null;
            ShippingStatus? parsedShippingStatus = null;

            if (dto.Status is not null)
            {
                if (!Enum.TryParse<OrderStatus>(dto.Status, ignoreCase: true, out var status))
                    throw new InvalidOperationException($"Invalid order status '{dto.Status}'.");
                order.Status = status;
                parsedStatus = status;
            }
            if (dto.PaymentStatus is not null)
            {
                if (!Enum.TryParse<PaymentStatus>(dto.PaymentStatus, ignoreCase: true, out var paymentStatus))
                    throw new InvalidOperationException($"Invalid payment status '{dto.PaymentStatus}'.");
                order.PaymentStatus = paymentStatus;
                parsedPaymentStatus = paymentStatus;
            }
            if (dto.ShippingStatus is not null)
            {
                if (!Enum.TryParse<ShippingStatus>(dto.ShippingStatus, ignoreCase: true, out var shippingStatus))
                    throw new InvalidOperationException($"Invalid shipping status '{dto.ShippingStatus}'.");
                order.ShippingStatus = shippingStatus;
                parsedShippingStatus = shippingStatus;
            }
            if (dto.ShippedAt.HasValue) order.ShippedAt = dto.ShippedAt;
            if (dto.DeliveredAt.HasValue) order.DeliveredAt = dto.DeliveredAt;

            if (parsedStatus == OrderStatus.Shipped && order.ShippedAt == null)
                order.ShippedAt = DateTime.UtcNow;
            if (parsedStatus == OrderStatus.Delivered && order.DeliveredAt == null)
                order.DeliveredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(order);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return false;

            order.DeletedAt = DateTime.UtcNow;
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
