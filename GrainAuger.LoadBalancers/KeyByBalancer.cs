using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers;

public class KeyByBalancer<T, TKey> : IAsyncObserver<T>
{
    private readonly IStreamProvider _streamProvider;
    private readonly Func<T, StreamId> _streamIdSelector;
    private readonly string _outputNamespace;

    public KeyByBalancer(string outputNamespace, IStreamProvider streamProvider, Func<T, TKey> keySelector)
    {
        _streamProvider = streamProvider;
        _outputNamespace = outputNamespace;
        _streamIdSelector = typeof(TKey) switch
        {
            { } t when t == typeof(long) => (Func<T, StreamId>)(item => GetStreamIdLong((long)(object)keySelector(item)!)),
            { } t when t == typeof(string) => (Func<T, StreamId>)(item => GetStreamIdString((string)(object)keySelector(item)!)),
            { } t when t == typeof(Guid) => (Func<T, StreamId>)(item => GetStreamIdGuid((Guid)(object)keySelector(item)!)),
            _ => throw new ArgumentException($"Unsupported key type {typeof(TKey)}")
        };
    }
    
    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        var streamId = _streamIdSelector(item);
        var stream = _streamProvider.GetStream<T>(streamId);
        return stream.OnNextAsync(item, token);
    }

    private StreamId GetStreamIdLong(long key)
    {
        return StreamId.Create(_outputNamespace, key);
    }
    
    private StreamId GetStreamIdString(string key)
    {
        return StreamId.Create(_outputNamespace, key);
    }
    
    private StreamId GetStreamIdGuid(Guid key)
    {
        return StreamId.Create(_outputNamespace, key);
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }
}