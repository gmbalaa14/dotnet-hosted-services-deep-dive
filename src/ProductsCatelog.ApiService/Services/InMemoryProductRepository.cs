using ProductsCatelog.ApiService.Models;

namespace ProductsCatelog.ApiService.Services;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private readonly object _lock = new();

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_products.AsEnumerable());
        }
    }

    public Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _products.AddRange(products);
        }

        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_products.Count);
        }
    }
}
