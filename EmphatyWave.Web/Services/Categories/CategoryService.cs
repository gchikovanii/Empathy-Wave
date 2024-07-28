using EmphatyWave.Domain;

namespace EmphatyWave.Web.Services.Categories
{
    public class CategoryService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        public async Task<ICollection<Category>> GetAllCategories()
        {
            var response = await _httpClient.GetFromJsonAsync<ICollection<Category>>($"https://localhost:7481/api/Category");
            return response;
        }
    }
}
