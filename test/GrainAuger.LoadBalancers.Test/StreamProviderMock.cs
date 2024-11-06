using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers.Test;

public class StreamProviderMock : IStreamProvider
{
    public Dictionary<StreamId, object> RecordedStreams { get; } = new();
    
    public IAsyncStream<T> GetStream<T>(StreamId streamId)
    {
        if (RecordedStreams.TryGetValue(streamId, out var item))
        {
            return (IAsyncStream<T>)item;
        }
        var stream = new AsyncStreamMock<T>(streamId);
        RecordedStreams[streamId] = stream;
        return stream;
    }

    public string Name => "StreamProviderMock";
    public bool IsRewindable => false;
}

