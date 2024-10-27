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
    public void TotalNumberOfItemsIsEqualToInput(int itemCount)
    {
        var loadBalancer = new RoundRobinLoadBalancer<int>("test", _streamProviderMock, 5);
        var items = new List<SequentialItem<int>>();
        for (int i = 0; i < itemCount; i++)
        {
            var seq = new SequentialItem<int>(i, new EventSequenceToken());
            items.Add(seq);
        }
        loadBalancer.OnNextAsync(items).Wait();
        
        int totalItems = _streamProviderMock.RecordedItems.Sum(x => ((AsyncStreamMock<int>)x).RecordedItems.Count);
        Assert.That(totalItems, Is.EqualTo(itemCount));
    }
    
    [TestCase(1, 100)]
    [TestCase(100, 1)]
    [TestCase(100, 5)]
    [TestCase(100, 100)]
    [TestCase(100, 1000)]
    public void ItemsAreDistributedEvenly(int itemCount, int numBuckets)
    {
        var loadBalancer = new RoundRobinLoadBalancer<int>("test", _streamProviderMock, numBuckets);
        var items = new List<SequentialItem<int>>();
        for (int i = 0; i < itemCount; i++)
        {
            var seq = new SequentialItem<int>(i, new EventSequenceToken());
            items.Add(seq);
        }
        loadBalancer.OnNextAsync(items).Wait();
        
        var recordedItems = _streamProviderMock.RecordedItems.Cast<AsyncStreamMock<int>>().ToList();
        var itemCounts = recordedItems.Select(x => x.RecordedItems.Count).ToList();
        var min = itemCounts.Min();
        var max = itemCounts.Max();
        Assert.That(max - min, Is.LessThanOrEqualTo(1));
    }
}