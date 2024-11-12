using Orleans.Providers.Streams.Common;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers.Test;

public class RoundRobinLoadBalancerTests
{
    private StreamProviderMock _streamProviderMock;

    [SetUp]
    public void Setup()
    {
        _streamProviderMock = new StreamProviderMock();
    }
    
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(1000)]
    public async Task TotalNumberOfItemsIsEqualToInput(int itemCount)
    {
        var loadBalancer = new RoundRobinLoadBalancer<int>("test", _streamProviderMock, 5);
        
        for (int i = 0; i < itemCount; i++)
        {
            await loadBalancer.OnNextAsync(i);
        }
        
        int totalItems = _streamProviderMock.RecordedStreams.Values.Sum(x => ((AsyncStreamMock<int>)x).RecordedItems.Count);
        Assert.That(totalItems, Is.EqualTo(itemCount));
    }
    
    [TestCase(1, 100)]
    [TestCase(100, 1)]
    [TestCase(100, 5)]
    [TestCase(100, 100)]
    [TestCase(100, 1000)]
    public async Task ItemsAreDistributedEvenly(int itemCount, int numBuckets)
    {
        var loadBalancer = new RoundRobinLoadBalancer<int>("test", _streamProviderMock, numBuckets);

        for (int i = 0; i < itemCount; i++)
        {
            await loadBalancer.OnNextAsync(i);
        }

        var recordedItems = _streamProviderMock.RecordedStreams.Values.Cast<AsyncStreamMock<int>>().ToList();
        var itemCounts = recordedItems.Select(x => x.RecordedItems.Count).ToList();
        var min = itemCounts.Min();
        var max = itemCounts.Max();
        Assert.That(max - min, Is.LessThanOrEqualTo(1));
    }
}