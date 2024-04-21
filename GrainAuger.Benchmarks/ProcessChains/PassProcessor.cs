using Orleans.Streams;

namespace GrainAuger.Benchmarks.ProcessChains;

public class PassProcessor(IAsyncObserver<int> output) : IAsyncObserver<int>
{
    public Task OnNextAsync(int item, StreamSequenceToken? token = null)
    {
        return output.OnNextAsync(item, token);
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
