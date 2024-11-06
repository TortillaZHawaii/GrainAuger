using Orleans.Providers.Streams.Common;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers.Test;

public class RandomLoadBalancerTests
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
        var loadBalancer = new RandomLoadBalancer<int>("test", _streamProviderMock, 5);
        var items = new List<SequentialItem<int>>();
        for (int i = 0; i < itemCount; i++)
        {
            var seq = new SequentialItem<int>(i, new EventSequenceToken());
            items.Add(seq);
        }
        loadBalancer.OnNextAsync(items).Wait();
        
        int totalItems = _streamProviderMock.RecordedStreams.Values.Sum(x => ((AsyncStreamMock<int>)x).RecordedItems.Count);
        Assert.That(totalItems, Is.EqualTo(itemCount));
    }
}
