using FluentValidation;
using System.Text.Json.Serialization;

namespace NovaStore.Application.DTOs
{
    // ========== Product DTOs ==========
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public bool IsActive { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
    }

    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).When(x => x.StockQuantity.HasValue);
            RuleFor(x => x.CategoryId).GreaterThan(0);
            RuleFor(x => x.ImageUrl).MaximumLength(500);
            RuleFor(x => x.Brand).MaximumLength(100);
        }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? StockQuantity { get; set; }
        public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Name).Length(1, 200).When(x => x.Name != null);
            RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
            RuleFor(x => x.Price).InclusiveBetween(0.01m, 9999999m).When(x => x.Price.HasValue);
            RuleFor(x => x.StockQuantity).InclusiveBetween(0, 999999).When(x => x.StockQuantity.HasValue);
            RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);
            RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl != null);
            RuleFor(x => x.Brand).MaximumLength(100).When(x => x.Brand != null);
        }
    }

    public class ProductSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Brand { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // ========== Category DTOs ==========
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new();
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.ImageUrl).MaximumLength(500);
        }
    }

    public class UpdateCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Name).Length(1, 100).When(x => x.Name != null);
            RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
            RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl != null);
        }
    }

    // ========== Cart DTOs ==========
    public class CartItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
    {
        public AddToCartDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDtoValidator : AbstractValidator<UpdateCartItemDto>
    {
        public UpdateCartItemDtoValidator()
        {
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        }
    }

    // ========== Order DTOs ==========
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? ShippingAddressId { get; set; }
        public string? ShippingNotes { get; set; }
        public string PaymentMethod { get; set; } = "Card";
        public string PaymentStatus { get; set; } = "Pending";
        public string ShippingStatus { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? PromoCode { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string Status { get; set; } = "Created";
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

    public class CreateOrderDto
    {
        public int? ShippingAddressId { get; set; }
        public string? ShippingNotes { get; set; }
        public string PaymentMethod { get; set; } = "Card";
        public string? PromoCode { get; set; }
    }

    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.ShippingNotes).MaximumLength(500);
            RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(50);
            RuleFor(x => x.PromoCode).MaximumLength(50);
        }
    }

    public class UpdateOrderDto
    {
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? ShippingStatus { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
    {
        private static readonly string[] ValidStatuses = { "Created", "Confirmed", "Shipped", "Delivered", "Cancelled" };
        private static readonly string[] ValidPaymentStatuses = { "Pending", "Paid", "Failed", "Refunded" };
        private static readonly string[] ValidShippingStatuses = { "Pending", "Shipped", "Delivered" };

        public UpdateOrderDtoValidator()
        {
            RuleFor(x => x.Status)
                .Must(s => ValidStatuses.Contains(s))
                .When(x => x.Status != null)
                .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
            RuleFor(x => x.PaymentStatus)
                .Must(s => ValidPaymentStatuses.Contains(s))
                .When(x => x.PaymentStatus != null)
                .WithMessage($"PaymentStatus must be one of: {string.Join(", ", ValidPaymentStatuses)}");
            RuleFor(x => x.ShippingStatus)
                .Must(s => ValidShippingStatuses.Contains(s))
                .When(x => x.ShippingStatus != null)
                .WithMessage($"ShippingStatus must be one of: {string.Join(", ", ValidShippingStatuses)}");
        }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    // ========== Review DTOs ==========
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Comment).MaximumLength(2000);
        }
    }

    // ========== User DTOs ==========
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class RegisterUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
    }

    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.Username).NotEmpty().Length(3, 50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
            RuleFor(x => x.Password).NotEmpty().Length(8, 100);
            RuleFor(x => x.FullName).MaximumLength(100);
            RuleFor(x => x.Phone).MaximumLength(20);
        }
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .Length(8, 100)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
        }
    }

    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FullName).MaximumLength(100).When(x => x.FullName != null);
            RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
            RuleFor(x => x.AvatarUrl).MaximumLength(500).When(x => x.AvatarUrl != null);
        }
    }

    // ========== Address DTOs ==========
    public class AddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Россия";
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAddressDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Россия";
        public bool IsDefault { get; set; }
    }

    public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
    {
        public CreateAddressDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.State).MaximumLength(100);
            RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        }
    }
}
