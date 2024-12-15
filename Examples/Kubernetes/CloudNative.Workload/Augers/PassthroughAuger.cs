using Orleans.Streams;

namespace CloudNative.Workload.Augers;

public class PassthroughAuger(IAsyncObserver<CardTransaction> output) : IAsyncObserver<CardTransaction>
{
    public async Task OnNextAsync(CardTransaction item, StreamSequenceToken? token = null)
    {
        await output.OnNextAsync(item);
    }

    public async Task OnCompletedAsync()
    {
        await output.OnCompletedAsync();
    }

    public async Task OnErrorAsync(Exception ex)
    {
        await output.OnCompletedAsync();
    }
}
