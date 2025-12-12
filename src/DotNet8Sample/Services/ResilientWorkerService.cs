using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace DotNet8Sample.Services;

internal class ResilientWorkerService(
    ILogger<ResilientWorkerService> logger,
    IOptionsMonitor<WorkerOptions> options
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                (exception, retryCount) =>
                    logger.LogError(exception, "Retry {retryCount} occurred.", retryCount));

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.CurrentValue.IntervalInSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                logger.LogInformation("Running resilient work at: {time}", DateTimeOffset.Now);

                if (new Random().Next(1, 4) == 1)
                    throw new Exception("Simulated failure.");

                await Task.Delay(1000, stoppingToken);
            });
        }
    }
}
