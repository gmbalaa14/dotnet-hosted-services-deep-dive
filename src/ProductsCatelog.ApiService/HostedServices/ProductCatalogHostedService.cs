using ProductsCatelog.ApiService.Services;

namespace ProductsCatelog.ApiService.HostedServices;

public class ProductCatalogHostedService(
    IServiceScopeFactory serviceScopeFactory, // Used to create scopes for resolving scoped services since hosted services are singletons
    ILogger<ProductCatalogHostedService> logger,
    IConfiguration configuration) : IHostedService, IDisposable // We implement IDisposable to clean up the CTS; otherwise it introduces a memory leak
{
    private CancellationTokenSource? _cts; // Cancellation token source to signal cancellation to the background task

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer starting.");

        // Create a linked CTS so we can cancel our background work when StopAsync is called.
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Start the background task and return; Otherwise this would block startup.
        Task.Run(() => DoWorkAsync(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
        //await DoWorkAsync(_cts.Token); // Asynchronous work during startup (not recommended) will delay the application from starting until this work is complete.
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Wait a small amount to simulate some startup delay and to allow graceful shutdown during start.
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

            using var scope = serviceScopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<ProductSeeder>();
            var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            var count = await repo.CountAsync(cancellationToken);
            if (count == 0)
            {
                logger.LogInformation("Seeding product catalog...");

                // Read seed count from configuration (fallback to 50)
                var configured = configuration.GetValue<int?>("ProductSeeder:SeedCount");
                var seedCount = configured.GetValueOrDefault(50);

                logger.LogInformation("Seeding product catalog with {SeedCount} products (configured: {Configured}).", seedCount, configured);

                var products = seeder.Generate(seedCount);
                await repo.AddRangeAsync(products, cancellationToken);
                logger.LogInformation("Seeded {Count} products.", products.Count());
            }
            else
            {
                logger.LogInformation("Product catalog already contains {Count} items.", count);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Product catalog initialization was cancelled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the product catalog.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer stopping.");

        if (_cts == null)
        {
            return;
        }

        // Signal cancellation to the executing method
        _cts.Cancel();

        logger.LogInformation("Product catalog initializer stopped.");
    }

    public void Dispose()
    {
        _cts?.Cancel();
    }
}
