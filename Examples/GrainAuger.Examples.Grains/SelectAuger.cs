using GrainAuger.Examples.Abstractions;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.Grains;

public interface ISelectAuger<out TInput, in TOutput> : IGrainWithStringKey
{
    public Task StartAsync(string providerName, string streamNamespace, Guid inputStreamGuid,
        Guid outputStreamGuid);

    public Task StopAsync();
}

public class ExampleSelectAuger : Grain, ISelectAuger<int, int>, IAsyncObserver<int>, IAugerGrain
{
    // CodecNotFoundException: Could not find a copier for type System.Func`2[System.Int32,System.Int32].
    private readonly Func<int, int> _selector = x => x * 2;
    private readonly ILogger _logger;

    private IAsyncStream<int>? _outputStream;
    private IAsyncObservable<int>? _inputStream;

    private StreamSubscriptionHandle<int>? _subscriptionHandle;

    public ExampleSelectAuger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger($"{this.GetType().Name}--{this.IdentityString}");
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OnActivateAsync");
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task StartAsync(string providerName, string streamNamespace, Guid inputStreamGuid, Guid outputStreamGuid)
    {
        _logger.LogInformation("StartAsync");

        var provider = this.GetStreamProvider(providerName);
        var inputStreamId = StreamId.Create(streamNamespace, inputStreamGuid);
        var outputStreamId = StreamId.Create(streamNamespace, outputStreamGuid);
        _inputStream = provider.GetStream<int>(inputStreamId);
        _outputStream = provider.GetStream<int>(outputStreamId);

        _subscriptionHandle = await _inputStream.SubscribeAsync(this);
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("StopAsync");
        if (_subscriptionHandle != null)
        {
            await _subscriptionHandle!.UnsubscribeAsync();
            _subscriptionHandle = null;
        }
    }

    // Required by IAsyncObserver<TInput>
    public async Task OnNextAsync(int item, StreamSequenceToken? token = null)
    {
        var output = _selector!(item);
        _logger.LogInformation("OnNextAsync: {Input} -> {Output}", item, output);
        await _outputStream!.OnNextAsync(output, token);
    }

    // Required by IAsyncObserver<TInput>
    public async Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "OnErrorAsync");
        await _outputStream!.OnErrorAsync(ex);
    }

    // Required by IAsyncObserver<TInput>
    public async Task OnCompletedAsync()
    {
        _logger.LogInformation("OnCompletedAsync");
        await _outputStream!.OnCompletedAsync();
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _logger.LogInformation("OnDeactivateAsync");
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}
