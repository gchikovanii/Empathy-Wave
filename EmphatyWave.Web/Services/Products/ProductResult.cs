using EmphatyWave.Domain;

namespace EmphatyWave.Web.Services.Products
{
    public class ProductResult
    {
        public List<Product> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
