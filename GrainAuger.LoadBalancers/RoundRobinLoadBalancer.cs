using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers;

public class RoundRobinLoadBalancer<T> : IAsyncBatchObserver<T> where T : class
{
    private readonly string _outputNamespace;
    private readonly IStreamProvider _streamProvider;

    private readonly int _numBuckets;
    private int _currentBucket;

    private List<StreamId> _buckets;

    RoundRobinLoadBalancer(string outputNamespace, IStreamProvider streamProvider, int numBuckets)
    {
        _outputNamespace = outputNamespace;
        _streamProvider = streamProvider;
        _numBuckets = numBuckets;
        _currentBucket = 0;
        _buckets = new List<StreamId>(numBuckets);
        for (int i = 0; i < numBuckets; i++)
        {
            _buckets.Add(StreamId.Create(_outputNamespace, i));
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

    public async Task OnNextAsync(IList<SequentialItem<T>> items)
    {
        foreach (var item in items)
        {
            var bucket = _buckets[_currentBucket];
            var stream = _streamProvider.GetStream<T>(bucket);
            await stream.OnNextAsync(item.Item);
            _currentBucket = _currentBucket + 1;
            if (_currentBucket >= _numBuckets)
            {
                _currentBucket = 0;
            }
        }
    }
}
