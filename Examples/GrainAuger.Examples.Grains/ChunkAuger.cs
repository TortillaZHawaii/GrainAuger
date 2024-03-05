using GrainAuger.Examples.Abstractions;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.Grains;

public interface IChunkAuger<in T> : IGrainWithStringKey
{
    public Task StartAsync(string providerName, string streamNamespace, Guid inputStreamGuid,
        Guid outputStreamGuid);

    public Task StopAsync();
}

public class ExampleChunkAuger : Grain, IChunkAuger<int>, IAsyncObserver<int>, IAugerGrain
{
    private readonly ILogger _logger;
    private readonly IPersistentState<List<int>> _state;
    private const int ChunkSize = 3;

    private IAsyncStream<List<int>>? _outputStream;
    private IAsyncObservable<int>? _inputStream;

    private StreamSubscriptionHandle<int>? _subscriptionHandle;

    public ExampleChunkAuger(ILoggerFactory loggerFactory,
        [PersistentState("ChunkAugerSet", "Auger")] IPersistentState<List<int>> state)
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
        _outputStream = provider.GetStream<List<int>>(outputStreamId);

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
        _state.State.Add(item);
        if (_state.State.Count >= ChunkSize)
        {
            await _outputStream!.OnNextAsync(_state.State, token);
            _state.State.Clear();
        }
        await _state.WriteStateAsync();
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