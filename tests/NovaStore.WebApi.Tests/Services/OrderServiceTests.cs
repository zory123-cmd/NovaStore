using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NovaStore.Application.DTOs;
using NovaStore.Application.Services;
using NovaStore.Domain.Models;
using NovaStore.Domain.Models.Enums;
using NovaStore.Infrastructure.Data;
using NovaStore.Infrastructure.Options;

namespace NovaStore.WebApi.Tests.Services
{
    /// <summary>
    /// Test-specific DbContext that configures RowVersion for SQLite compatibility.
    /// SQLite does not support the [Timestamp] attribute natively, so we set a default value of 0.
    /// </summary>
    public class TestNovaStoreDbContext : NovaStoreDbContext
    {
        public TestNovaStoreDbContext(DbContextOptions<NovaStoreDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SQLite doesn't support [Timestamp] / rowversion natively;
            // set a default value so inserts don't fail with NOT NULL constraint.
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.RowVersion).HasDefaultValue(0);
            });
        }
    }

    public class OrderServiceTests
    {
        /// <summary>
        /// Creates a new in-memory SQLite database for each test.
        /// SQLite is used instead of EF Core InMemory because OrderService
        /// uses ExecuteSqlRawAsync for atomic stock decrement, which is
        /// not supported by the InMemory provider.
        /// </summary>
        private NovaStoreDbContext CreateDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<NovaStoreDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new TestNovaStoreDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private OrderService CreateOrderService(NovaStoreDbContext context)
        {
            var promoCodes = Options.Create(new PromoCodesSettings
            {
                Codes = new Dictionary<string, PromoCodeConfig>
                {
                    ["SAVE10"] = new PromoCodeConfig { Type = "PERCENTAGE", Value = 10 },
                    ["FLAT50"] = new PromoCodeConfig { Type = "FIXED", Value = 50 }
                }
            });
            return new OrderService(context, promoCodes);
        }

        [Fact]
        public async Task GetById_ExistingOrder_ReturnsOrderDto()
        {
            using var context = CreateDbContext();
            var user = new User { Username = "test", Email = "test@test.com", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Orders.Add(new Order
            {
                UserId = user.Id,
                CustomerName = "Test",
                TotalAmount = 100,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Created
            });
            await context.SaveChangesAsync();

            var service = CreateOrderService(context);
            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(100, result.TotalAmount);
        }

        [Fact]
        public async Task CreateFromCart_EmptyCart_ThrowsException()
        {
            using var context = CreateDbContext();
            var user = new User { Username = "test", Email = "test@test.com", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateOrderService(context);
            var dto = new CreateOrderDto { PaymentMethod = "Card" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateFromCartAsync(user.Id, dto));
        }

        [Fact]
        public async Task CreateFromCart_WithItems_CreatesOrder()
        {
            using var context = CreateDbContext();
            var user = new User { Username = "test", Email = "test@test.com", PasswordHash = "hash" };
            context.Users.Add(user);
            var category = new Category { Name = "Test" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                CategoryId = category.Id,
                IsActive = true
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            context.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 2 });
            await context.SaveChangesAsync();

            var service = CreateOrderService(context);
            var dto = new CreateOrderDto { PaymentMethod = "Card" };
            var result = await service.CreateFromCartAsync(user.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(200, result.TotalAmount); // 2 * 100
            Assert.Single(result.OrderItems);

            // Verify stock decreased (reload from DB to bypass change tracker cache)
            context.ChangeTracker.Clear();
            var updatedProduct = await context.Products.FindAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(8, updatedProduct.StockQuantity);
        }

        [Fact]
        public async Task CreateFromCart_WithPromoCode_AppliesDiscount()
        {
            using var context = CreateDbContext();
            var user = new User { Username = "test", Email = "test@test.com", PasswordHash = "hash" };
            context.Users.Add(user);
            var category = new Category { Name = "Test" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                CategoryId = category.Id,
                IsActive = true
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            context.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 2 });
            await context.SaveChangesAsync();

            var service = CreateOrderService(context);
            var dto = new CreateOrderDto { PaymentMethod = "Card", PromoCode = "SAVE10" }; // 10% off
            var result = await service.CreateFromCartAsync(user.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(180, result.TotalAmount); // 200 - 10%
            Assert.Equal(20, result.DiscountAmount);
        }

        [Fact]
        public async Task CancelOrder_ReturnsStock()
        {
            using var context = CreateDbContext();
            var user = new User { Username = "test", Email = "test@test.com", PasswordHash = "hash" };
            context.Users.Add(user);
            var category = new Category { Name = "Test" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                Price = 100,
                StockQuantity = 5,
                CategoryId = category.Id,
                IsActive = true
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var order = new Order
            {
                UserId = user.Id,
                CustomerName = "Test",
                TotalAmount = 200,
                Status = OrderStatus.Created,
                OrderDate = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            context.OrderItems.Add(new OrderItem { OrderId = order.Id, ProductId = product.Id, ProductName = "Test", UnitPrice = 100, Quantity = 2 });
            await context.SaveChangesAsync();

            var service = CreateOrderService(context);
            var result = await service.CancelOrderAsync(order.Id, user.Id);

            Assert.Equal("Cancelled", result.Status);

            var updatedProduct = await context.Products.FindAsync(product.Id);
            Assert.Equal(7, updatedProduct!.StockQuantity); // 5 + 2
        }

        [Fact]
        public async Task UpdateStatus_InvalidTransition_ThrowsException()
        {
            using var context = CreateDbContext();
            var user = new User { Username = "test", Email = "test@test.com", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var order = new Order
            {
                UserId = user.Id,
                CustomerName = "Test",
                TotalAmount = 100,
                Status = OrderStatus.Delivered,
                OrderDate = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var service = CreateOrderService(context);
            var dto = new UpdateOrderDto { Status = "Created" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStatusAsync(order.Id, dto));
        }
    }
}
