namespace GrainAuger.LoadBalancers.Test;

public class KeyByBalancerTests
{
    private StreamProviderMock _streamProviderMock;
    
    [SetUp]
    public void Setup()
    {
        _streamProviderMock = new StreamProviderMock();
    }
    
    private record ExampleDto(int Id, string Name, Guid Guid);

    // count / 2 ids, 5 names, 3 guids
    private List<ExampleDto> GenerateExampleDtos(int count)
    {
        var names = new List<string> { "Alice", "Bob", "Charlie", "David", "Eve" };
        var guids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        
        var exampleDtos = new List<ExampleDto>(count);
        for (int i = 0; i < count; i++)
        {
            var id = i % (count + 1) / 2;
            var name = names[i % names.Count];
            var guid = guids[i % guids.Count];
            exampleDtos.Add(new ExampleDto(id, name, guid));
        }
        
        return exampleDtos;
    }
    
    [TestCase(1, 1)]
    [TestCase(100, 50)]
    [TestCase(1000, 500)]
    public async Task KeyByBalancer_LongKey(int count, int numBuckets)
    {
        var exampleDtos = GenerateExampleDtos(count);
        var loadBalancer = new KeyByBalancer<ExampleDto, long>("test", _streamProviderMock, x => x.Id);
        foreach (var exampleDto in exampleDtos)
        {
            await loadBalancer.OnNextAsync(exampleDto);
        }
        
        Assert.That(_streamProviderMock.RecordedStreams.Count, Is.EqualTo(numBuckets));
        
        var streams = _streamProviderMock.RecordedStreams.Values.Cast<AsyncStreamMock<ExampleDto>>().ToList();
        var itemCounts = streams.Select(x => x.RecordedItems.Count).ToList();
        var min = itemCounts.Min();
        var max = itemCounts.Max();
        Assert.That(max - min, Is.LessThanOrEqualTo(1));
    }
    
    [TestCase(1, 1)]
    [TestCase(100, 5)]
    [TestCase(1000, 5)]
    public async Task KeyByBalancer_StringKey(int count, int numBuckets)
    {
        var exampleDtos = GenerateExampleDtos(count);
        var loadBalancer = new KeyByBalancer<ExampleDto, string>("test", _streamProviderMock, x => x.Name);
        foreach (var exampleDto in exampleDtos)
        {
            await loadBalancer.OnNextAsync(exampleDto);
        }

        Assert.That(_streamProviderMock.RecordedStreams.Count, Is.EqualTo(numBuckets));
        
        var streams = _streamProviderMock.RecordedStreams.Values.Cast<AsyncStreamMock<ExampleDto>>().ToList();
        var itemCounts = streams.Select(x => x.RecordedItems.Count).ToList();
        var min = itemCounts.Min();
        var max = itemCounts.Max();
        Assert.That(max - min, Is.LessThanOrEqualTo(1));
    }
    
    [TestCase(1, 1)]
    [TestCase(100, 3)]
    [TestCase(1000, 3)]
    public async Task KeyByBalancer_GuidKey(int count, int numBuckets)
    {
        var exampleDtos = GenerateExampleDtos(count);
        var loadBalancer = new KeyByBalancer<ExampleDto, Guid>("test", _streamProviderMock, x => x.Guid);
        foreach (var exampleDto in exampleDtos)
        {
            await loadBalancer.OnNextAsync(exampleDto);
        }

        Assert.That(_streamProviderMock.RecordedStreams.Count, Is.EqualTo(numBuckets));
        
        var streams = _streamProviderMock.RecordedStreams.Values.Cast<AsyncStreamMock<ExampleDto>>().ToList();
        var itemCounts = streams.Select(x => x.RecordedItems.Count).ToList();
        var min = itemCounts.Min();
        var max = itemCounts.Max();
        Assert.That(max - min, Is.LessThanOrEqualTo(1));
    }
}