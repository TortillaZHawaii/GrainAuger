using Orleans.Streams;

namespace WordCount;

public interface IFeatureFlagService
{
    public Task<bool> IsEnabled(string featureFlag);
}

public class FeatureFlagService : IFeatureFlagService
{
    public bool Enabled { get; set; } = true;
    
    public Task<bool> IsEnabled(string featureFlag)
    {
        return Task.FromResult(Enabled);
    }
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
    
    public Task OnCompletedAsync()
    {
        throw new NotImplementedException();
    }

    public Task OnErrorAsync(Exception ex)
    {
        throw new NotImplementedException();
    }
}