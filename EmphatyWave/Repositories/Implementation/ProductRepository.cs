using EmphatyWave.Domain;
using EmphatyWave.Persistence.DataContext;
using EmphatyWave.Persistence.Models;
using EmphatyWave.Persistence.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace EmphatyWave.Persistence.Repositories.Implementation
{
    public class ProductRepository(IBaseRepository<Product> repository, ApplicationDbContext context) : IProductRepository
    {
        private readonly IBaseRepository<Product> _repository = repository;
        private readonly ApplicationDbContext _context = context;

        public async Task<ICollection<Product>> GetProducts(CancellationToken token, int pageNumber, int pageSize)
        {
            return await _repository.GetPaginatedData(token, pageNumber, pageSize).ConfigureAwait(false);
        }
        public async Task<PagedResult<Product>> GetFilteredProducts(CancellationToken token, int pageNumber, int pageSize, decimal? minValue, decimal? maxValue, string? categoryName, string? searchKeyword)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(i => i.Images)
                .Include(i => i.Category)
                .AsQueryable();
            if (minValue.HasValue && maxValue.HasValue && minValue > maxValue)
            {
                return new PagedResult<Product>
                {
                    Items = new List<Product>(),
                    TotalCount = 0
                };
            }
            if (minValue.HasValue)
                query = query.Where(i => i.Price >= minValue.Value);
            if (maxValue.HasValue)
                query = query.Where(i => i.Price <= maxValue.Value);
            if (categoryName != null)
                query = query.Where(i => i.Category.Name == categoryName);
            if (!string.IsNullOrEmpty(searchKeyword))
                query = query.Where(i => i.Name.Contains(searchKeyword));

            var totalCount = await query.CountAsync(token);
            var products = await query
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).OrderBy(i => i.Price).ToListAsync(token).ConfigureAwait(false);
            return new PagedResult<Product>
            {
                Items = products,
                TotalCount = totalCount
            };
        }
        public async Task<Product> GetProductByName(CancellationToken token, string productName)
        {
            return await _repository.GetQuery(i => i.Name == productName).FirstOrDefaultAsync(token).ConfigureAwait(false) ?? new Product { };
        }
        public async Task<Product> GetProductById(CancellationToken token, Guid productId)
        {
            var product = await _repository.GetDataById(token, productId).ConfigureAwait(false);
            return product ?? new Product { };
        }

        public async Task CreateProductAsync(CancellationToken token, Product product)
        {
            await _repository.CreateData(token, product).ConfigureAwait(false);
        }
        public void UpdateProduct(Product product)
        {
            _repository.UpdateData(product);
        }
        public void DeleteProduct(Product product)
        {
            _repository.DeleteData(product);
        }
    }
}
