using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet8Sample.Services;

internal class BackgroundTimerService(ILogger<BackgroundTimerService> logger) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BackgroundTimerService executing.");

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogInformation("Tick: {time}", DateTimeOffset.Now);
        }
    }
}
