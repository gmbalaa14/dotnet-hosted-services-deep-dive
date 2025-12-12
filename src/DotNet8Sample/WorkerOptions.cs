namespace DotNet8Sample;

internal class WorkerOptions
{
    public int MaxRetryCount { get; set; } = 3;
    public int IntervalInSeconds { get; set; } = 5;
}
