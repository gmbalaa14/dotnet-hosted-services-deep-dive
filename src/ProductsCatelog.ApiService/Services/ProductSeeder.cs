using Bogus;
using ProductsCatelog.ApiService.Models;

namespace ProductsCatelog.ApiService.Services;

public class ProductSeeder
{
    private readonly Faker<Product> _faker;

    public ProductSeeder()
    {
        _faker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker + 1)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1, 1000)))
            .RuleFor(p => p.Category, f => f.Commerce.Categories(1).First());
    }

    public IEnumerable<Product> Generate(int count)
    {
        return _faker.Generate(count);
    }
}
