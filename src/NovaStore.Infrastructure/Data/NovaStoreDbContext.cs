using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NovaStore.Domain.Models;
using NovaStore.Domain.Models.Enums;

namespace NovaStore.Infrastructure.Data
{
    public class NovaStoreDbContext : DbContext
    {
        public NovaStoreDbContext(DbContextOptions<NovaStoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Address> Addresses => Set<Address>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var orderStatusConverter = new ValueConverter<OrderStatus, string>(
                v => v.ToString(), v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v));
            var paymentStatusConverter = new ValueConverter<PaymentStatus, string>(
                v => v.ToString(), v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v));
            var shippingStatusConverter = new ValueConverter<ShippingStatus, string>(
                v => v.ToString(), v => (ShippingStatus)Enum.Parse(typeof(ShippingStatus), v));

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasDefaultValue("Customer");
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.CategoryId);
                entity.HasIndex(p => p.IsActive);
                entity.Property(p => p.IsActive).HasDefaultValue(true);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.OrderDate);
                entity.Property(o => o.Status).HasConversion(orderStatusConverter).HasDefaultValue(OrderStatus.Created).HasColumnType("varchar(50)");
                entity.Property(o => o.PaymentStatus).HasConversion(paymentStatusConverter).HasDefaultValue(PaymentStatus.Pending).HasColumnType("varchar(50)");
                entity.Property(o => o.ShippingStatus).HasConversion(shippingStatusConverter).HasDefaultValue(ShippingStatus.Pending).HasColumnType("varchar(50)");
                entity.Property(o => o.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.ShippingAddress)
                    .WithMany()
                    .HasForeignKey(o => o.ShippingAddressId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(o => o.DeletedAt == null);
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(ci => new { ci.UserId, ci.ProductId }).IsUnique();

                entity.HasOne(ci => ci.User)
                    .WithMany(u => u.CartItems)
                    .HasForeignKey(ci => ci.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();

                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Address
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
