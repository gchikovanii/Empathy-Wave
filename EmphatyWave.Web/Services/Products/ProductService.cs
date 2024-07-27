using EmphatyWave.Domain;

namespace EmphatyWave.Web.Services.Products
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<Product>> GetFilteredProducts(int pageNumber, int pageSize, decimal? minPrice, decimal? maxPrice, string? categoryName, string? searchKeyword)
        {
            var response = await _httpClient.GetFromJsonAsync<PagedResult<Product>>($"https://localhost:7481/api/Product?pageSize={pageSize}&pageNumber={pageNumber}&maxPrice={maxPrice}&minPrice={minPrice}&categoryName={categoryName}&searchKeyword={searchKeyword}");
            return response;
        }
    }
}
