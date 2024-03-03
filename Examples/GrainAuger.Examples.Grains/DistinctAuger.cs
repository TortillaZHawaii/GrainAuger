using GrainAuger.Examples.Abstractions;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.Grains;

public interface IDistinctAuger<in T> : IGrainWithStringKey
{
    public Task StartAsync(string providerName, string streamNamespace, Guid inputStreamGuid,
        Guid outputStreamGuid);

    public Task StopAsync();
}

public class ExampleDistinctAuger : Grain, IDistinctAuger<int>, IAsyncObserver<int>, IAugerGrain
{
    private readonly ILogger _logger;

    private IAsyncStream<int>? _outputStream;
    private IAsyncObservable<int>? _inputStream;

    private readonly IPersistentState<HashSet<int>> _state;

    private StreamSubscriptionHandle<int>? _subscriptionHandle;

    public ExampleDistinctAuger(
        [PersistentState("ExampleDistinctAugerSet", "Auger")] IPersistentState<HashSet<int>> state,
        ILoggerFactory loggerFactory)
    {
        _state = state;
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
        _logger.LogInformation("OnNextAsync: {Input}", item);
        if (!_state.State.Add(item))
        {
            _logger.LogInformation("OnNextAsync: {Input} already seen", item);
            return;
        }

        await Task.WhenAll(
            _outputStream!.OnNextAsync(item, token),
            _state.WriteStateAsync()
        );
    }

    public Task OnCompletedAsync()
    {
        return _outputStream!.OnCompletedAsync();
    }

    public Task OnErrorAsync(Exception ex)
    {
        return _outputStream!.OnErrorAsync(ex);
    }
}