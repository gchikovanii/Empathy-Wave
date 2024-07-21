using EmphatyWave.Domain.Localization;
using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Domain
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<OrderItem> OrderItems { get; set; }

        [Range(0, 50000, ErrorMessage = nameof(ErrorMessages.TotalPriceRange))]
        public decimal TotalAmount { get; set; }
        public ShippingDetail ShippingDetails { get; set; }
        public Status Status { get; set; }
        public string UserId { get; set; }
        public string? StripeToken { get; set; }
        public User User { get; set; }
        public bool IsEmpty => Id == Guid.Empty && string.IsNullOrEmpty(UserId);
    }
}
