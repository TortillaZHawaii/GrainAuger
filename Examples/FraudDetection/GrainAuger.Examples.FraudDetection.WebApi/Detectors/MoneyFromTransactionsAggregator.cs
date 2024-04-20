using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class MoneyFromTransactionsAggregator(IAsyncObserver<int> output, ILogger<MoneyFromTransactionsAggregator> logger) : IAsyncObserver<int>
{
    private int _sum = 0;
    
    public async Task OnNextAsync(int item, StreamSequenceToken? token = null)
    {
        _sum += item;
        logger.LogInformation("Sum: {Sum}", _sum);
        await output.OnNextAsync(_sum, token);
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