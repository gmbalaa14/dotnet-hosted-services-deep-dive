using ProductsCatelog.ApiService.Services;

namespace ProductsCatelog.ApiService.HostedServices;

public class ProductCatalogHostedLifecycleService(
    IServiceScopeFactory serviceScopeFactory,
    IHostApplicationLifetime lifetime,
    ILogger<ProductCatalogHostedLifecycleService> logger,
    IConfiguration configuration) : IHostedLifecycleService
{
    private CancellationTokenSource? _cts;
    private CancellationTokenRegistration? _startedRegistration;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer (with IHostApplicationLifetime) starting.");

        // Create a linked CTS so we observe both the provided cancellation token and host stopping token.
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.ApplicationStopping);

        // Run when the host signals it has started
        _startedRegistration = lifetime.ApplicationStarted.Register(async () =>
        {
            await DoWorkAsync(_cts.Token);
        });

        return Task.CompletedTask;
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Small delay to simulate work and allow graceful shutdown during start.
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

            using var scope = serviceScopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<ProductSeeder>();
            var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            var count = await repo.CountAsync(cancellationToken);
            if (count == 0)
            {
                logger.LogInformation("Seeding product catalog...");

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer (with IHostApplicationLifetime) stopping.");

        if (_cts == null)
        {
            return Task.CompletedTask;
        }

        _cts.Cancel();

        logger.LogInformation("Product catalog initializer stopped.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _startedRegistration?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer (with IHostApplicationLifetime) has started.");
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer (with IHostApplicationLifetime) is starting.");
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer (with IHostApplicationLifetime) has stopped.");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Product catalog initializer (with IHostApplicationLifetime) is stopping.");
        return Task.CompletedTask;
    }
}
