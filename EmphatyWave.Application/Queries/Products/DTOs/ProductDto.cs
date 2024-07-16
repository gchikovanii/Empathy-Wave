using EmphatyWave.Domain;
using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Application.Queries.Products.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public string SKU { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public Guid CategoryId { get; set; }
    }
}
