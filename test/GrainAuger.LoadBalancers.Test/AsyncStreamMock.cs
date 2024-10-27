using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.LoadBalancers.Test;

public class AsyncStreamMock<T>(StreamId streamId) : IAsyncStream<T>
{
    public List<T> RecordedItems { get; } = new();
    
    public Task<IList<StreamSubscriptionHandle<T>>> GetAllSubscriptionHandles()
    {
        throw new NotImplementedException();
    }

    public bool IsRewindable => throw new NotImplementedException();
    public string ProviderName => throw new NotImplementedException();
    public StreamId StreamId => streamId;
    public bool Equals(IAsyncStream<T>? other)
    {
        return other?.StreamId.Equals(StreamId) ?? false;
    }

    public int CompareTo(IAsyncStream<T>? other)
    {
        if (other == null)
        {
            return 1;
        }
        return StreamId.CompareTo(other.StreamId);
    }

    public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncObserver<T> observer)
    {
        throw new NotImplementedException();
    }

    public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncObserver<T> observer, StreamSequenceToken? token, string? filterData = null)
    {
        throw new NotImplementedException();
    }

    public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncBatchObserver<T> observer)
    {
        throw new NotImplementedException();
    }

    public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncBatchObserver<T> observer, StreamSequenceToken? token)
    {
        throw new NotImplementedException();
    }

    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        RecordedItems.Add(item);
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }

    public Task OnNextBatchAsync(IEnumerable<T> batch, StreamSequenceToken? token = null)
    {
        foreach (var item in batch)
        {
            RecordedItems.Add(item);
        }
        return Task.CompletedTask;
    }
}