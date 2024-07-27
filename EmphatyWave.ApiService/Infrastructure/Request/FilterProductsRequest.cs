namespace EmphatyWave.ApiService.Infrastructure.Request
{
    public class FilterProductsRequest
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? CategoryName { get; set; }
    }
}
