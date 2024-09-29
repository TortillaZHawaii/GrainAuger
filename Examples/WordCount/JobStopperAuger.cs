using Orleans.Streams;

namespace WordCount;

public interface IFeatureFlagService
{
    public Task<bool> IsEnabled(string featureFlag);
}

public class JobStopperAuger(IAsyncObserver<string> output, IFeatureFlagService featureFlagService) : IAsyncObserver<string>
{
    public async Task OnNextAsync(string item, StreamSequenceToken? token = null)
    {
        if (await featureFlagService.IsEnabled("WordFilterJob"))
        {
            await output.OnNextAsync(item);
        }
    }
    
    /* ... */
    
    public async Task OnCompletedAsync()
    {
        throw new NotImplementedException();
    }

    public async Task OnErrorAsync(Exception ex)
    {
        throw new NotImplementedException();
    }
}