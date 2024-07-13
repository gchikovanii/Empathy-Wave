namespace EmphatyWave.Domain
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<OrderItem> OrderItems { get; set; }
        public decimal TotalAmount { get; set; }
        public ShippingDetail ShippingDetails { get; set; }
        public Status Status { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
