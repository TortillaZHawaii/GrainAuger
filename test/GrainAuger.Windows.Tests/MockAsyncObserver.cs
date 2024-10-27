using Orleans.Streams;

namespace GrainAuger.Windows.Tests;

public class MockAsyncObserver<T> : IAsyncObserver<List<T>>
{
    public List<T> RecordedItems { get; } = new();
    
    public Task OnNextAsync(List<T> item, StreamSequenceToken? token = null)
    {
        RecordedItems.AddRange(item);
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
}