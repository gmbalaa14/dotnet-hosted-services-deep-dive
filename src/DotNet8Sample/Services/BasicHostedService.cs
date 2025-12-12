using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet8Sample.Services;

internal class BasicHostedService(ILogger<BasicHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BasicHostedService started at {time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BasicHostedService stopping at {time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }
}
