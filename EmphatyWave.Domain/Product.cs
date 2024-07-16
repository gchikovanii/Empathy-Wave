using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Domain
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Name is Required!")]
        [MinLength(2, ErrorMessage = "Nmae must contain at least 2 characters!")]

        public string Name { get; set; }
        [Required(ErrorMessage = "Title is Required!")]
        [MinLength(35, ErrorMessage = "Title must Maximum of  35 characters!")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is Required!")]
        [MinLength(15,ErrorMessage = "Description must contain at least 15 characters!")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Price is Required!")]
        [Range(1, 35000, ErrorMessage = "Max price is 50K")]
        public decimal Price { get; set; }
        [Range(1, 35000, ErrorMessage = "Max dicount price is 3K")]
        public decimal? Discount { get; set; }
        [Required(ErrorMessage = "Quantity is Required!")]
        [Range(1, 50000, ErrorMessage = "Max price is 50K")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "SKU is Required!")]
        [MinLength(5,ErrorMessage = "SKU is At least 5 in length!")]
        public string SKU { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
