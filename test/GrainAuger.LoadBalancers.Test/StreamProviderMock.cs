using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers.Test;

public class StreamProviderMock : IStreamProvider
{
    public List<object> RecordedItems { get; } = new();
    
    public IAsyncStream<T> GetStream<T>(StreamId streamId)
    {
        var stream = new AsyncStreamMock<T>(streamId);
        RecordedItems.Add(stream);
        return stream;
    }

    public string Name => "StreamProviderMock";
    public bool IsRewindable => false;
}

