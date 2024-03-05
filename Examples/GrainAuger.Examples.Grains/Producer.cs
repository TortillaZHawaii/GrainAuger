using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.Grains;

public interface IProducerGrain : IGrainWithStringKey
{
    Task StartAsync(string providerName, string streamNamespace,
        Guid outputStreamGuid);

    Task StopAsync();
}

public class ProducerGrain : Grain, IProducerGrain
{
    private readonly ILogger _logger;
    private IDisposable? _timer;
    private IAsyncStream<int>? _outputStream;

    public ProducerGrain(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger($"{this.GetType().Name}--{this.IdentityString}");
    }

    public Task StartAsync(string providerName, string streamNamespace, Guid outputStreamGuid)
    {
        _logger.LogInformation("StartAsync");
        _outputStream = this.GetStreamProvider(providerName)
            .GetStream<int>(StreamId.Create(streamNamespace, outputStreamGuid));
        _timer = this.RegisterTimer(OnTimer, null!, TimeSpan.Zero, TimeSpan.FromMicroseconds(100));
        return Task.CompletedTask;
    }

    private async Task OnTimer(object? state)
    {
        var value = new Random().Next(1, 10);
        _logger.LogInformation("OnTimer: {Value}", value);
        await _outputStream!.OnNextAsync(value);
    }

    public Task StopAsync()
    {
        _logger.LogInformation("StopAsync");
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }
}