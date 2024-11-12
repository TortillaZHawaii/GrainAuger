using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers;

public class RoundRobinLoadBalancer<T> : IAsyncObserver<T>
{
    private readonly IStreamProvider _streamProvider;

    private readonly int _numBuckets;
    private int _currentBucket;

    private readonly List<StreamId> _buckets;

    public RoundRobinLoadBalancer(string outputNamespace, IStreamProvider streamProvider, int numBuckets)
    {
        _streamProvider = streamProvider;
        _numBuckets = numBuckets;
        _currentBucket = 0;
        _buckets = new List<StreamId>(numBuckets);
        for (int i = 0; i < numBuckets; i++)
        {
            _buckets.Add(StreamId.Create(outputNamespace, i));
        }
    }

    public async Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        var bucket = _buckets[_currentBucket];
        var stream = _streamProvider.GetStream<T>(bucket);
        await stream.OnNextAsync(item);
        ++_currentBucket;
        if (_currentBucket >= _numBuckets)
        {
            _currentBucket = 0;
        }
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
