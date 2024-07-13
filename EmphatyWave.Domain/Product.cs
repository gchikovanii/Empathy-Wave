namespace EmphatyWave.Domain
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; } 
        public int Quantity { get; set; }
        public string SKU { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
