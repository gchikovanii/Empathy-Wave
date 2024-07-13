using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Domain
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Name is Required!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description is Required!")]
        [MinLength(15,ErrorMessage = "Description must contain at least 15 characters!")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Price is Required!")]
        [Range(1, 50000, ErrorMessage = "Max price is 50K")]
        public decimal Price { get; set; }
        [Range(1, 3000, ErrorMessage = "Max dicount price is 3K")]
        public decimal? Discount { get; set; }
        [Required(ErrorMessage = "Quantity is Required!")]
        [Range(1, 50000, ErrorMessage = "Max price is 50K")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "SKU is Required!")]
        public string SKU { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
