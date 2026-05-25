using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NovaStore.Domain.Models.Enums;

namespace NovaStore.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        public int? ShippingAddressId { get; set; }
        public Address? ShippingAddress { get; set; }

        [MaxLength(500)]
        public string? ShippingNotes { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Card";

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; }

        [MaxLength(50)]
        public string? PromoCode { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public Enums.OrderStatus Status { get; set; } = Enums.OrderStatus.Created;
        public Enums.PaymentStatus PaymentStatus { get; set; } = Enums.PaymentStatus.Pending;
        public Enums.ShippingStatus ShippingStatus { get; set; } = Enums.ShippingStatus.Pending;

        public DateTime? DeletedAt { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
