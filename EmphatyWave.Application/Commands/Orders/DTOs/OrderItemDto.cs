using EmphatyWave.Domain;

namespace EmphatyWave.Application.Commands.Orders.DTOs
{
    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Guid OrderId { get; set; }
    }
}
