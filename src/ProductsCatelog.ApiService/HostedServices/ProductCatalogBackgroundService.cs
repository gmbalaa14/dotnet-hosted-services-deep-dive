using ProductsCatelog.ApiService.Services;

namespace ProductsCatelog.ApiService.HostedServices;

public class ProductCatalogBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ProductCatalogBackgroundService> logger,
    IConfiguration configuration) : BackgroundService, IDisposable // BackgroundService is a abstract class that already implements IHostedService and IDisposable
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog background service starting.");
        return base.StartAsync(cancellationToken); // We should call the base implementation to ensure proper startup; it will trigger the ExecuteAsync method.
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Product catalog background service starting.");

        // --- Simulation of pre-.NET 10 behavior ---
        // Any synchronous work done before the first 'await' will run synchronously on the
        // calling thread during StartAsync. Prior to .NET 10 this meant it could block
        // the main thread and delay other services from starting. We simulate that here
        // by performing a synchronous sleep before the first await.
        var syncBlocking = TimeSpan.FromSeconds(15);
        logger.LogWarning("Simulating synchronous startup work for {Duration}. This will block the main thread until the first await.", syncBlocking);
        Thread.Sleep(syncBlocking); // <-- synchronous blocking portion (runs on the main thread before first await)

        try
        {
            // From this first await onward, execution continues on a threadpool thread.
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            using var scope = serviceScopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<ProductSeeder>();
            var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            var count = await repo.CountAsync(stoppingToken);
            if (count == 0)
            {
                logger.LogInformation("Seeding product catalog...");

                // Read seed count from configuration (fallback to 50)
                var configured = configuration.GetValue<int?>("ProductSeeder:SeedCount");
                var seedCount = configured.GetValueOrDefault(50);

                logger.LogInformation("Seeding product catalog with {SeedCount} products (configured: {Configured}).", seedCount, configured);

                var products = seeder.Generate(seedCount);
                await repo.AddRangeAsync(products, stoppingToken);
                logger.LogInformation("Seeded {Count} products.", products.Count());
            }
            else
            {
                logger.LogInformation("Product catalog already contains {Count} items.", count);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Product catalog background initialization was cancelled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the product catalog.");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog background service stopping.");
        return base.StopAsync(cancellationToken); // We should call the base implementation to ensure proper shutdown; it will trigger cancellation of the ExecuteAsync method.
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
