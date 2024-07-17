using EmphatyWave.Domain;

namespace EmphatyWave.Application.Queries.Orders.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public decimal TotalAmount { get; set; }
        public ShippingDetail ShippingDetails { get; set; }
        public Status Status { get; set; }
        public string UserId { get; set; }
    }
}
