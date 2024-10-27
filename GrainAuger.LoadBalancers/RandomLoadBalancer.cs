using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers;

public class RandomLoadBalancer<T> : IAsyncBatchObserver<T>
{
    private readonly IStreamProvider _streamProvider;

    private readonly int _numBuckets;
    private readonly Random _random;

    private readonly List<StreamId> _buckets;

    public RandomLoadBalancer(string outputNamespace, IStreamProvider streamProvider, int numBuckets)
    {
        _streamProvider = streamProvider;
        _numBuckets = numBuckets;
        _random = new Random();
        _buckets = new List<StreamId>(numBuckets);
        for (int i = 0; i < numBuckets; i++)
        {
            _buckets.Add(StreamId.Create(outputNamespace, i));
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
            int bucketIndex = _random.Next(_numBuckets);
            var bucket = _buckets[bucketIndex];
            var stream = _streamProvider.GetStream<T>(bucket);
            await stream.OnNextAsync(item.Item);
        }
    }
}