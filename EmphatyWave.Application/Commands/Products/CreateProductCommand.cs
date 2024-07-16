using EmphatyWave.Domain;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Application.Commands.Products
{
    public class CreateProductCommand : IRequest<bool>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public string SKU { get; set; }
        public Guid CategoryId { get; set; }
    }
}
