using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class MoneyFromTransactionGetter(IAsyncObserver<int> output) : IAsyncObserver<CardTransaction>
{
    public Task OnNextAsync(CardTransaction item, StreamSequenceToken? token = null)
    {
        return output.OnNextAsync(item.Amount, token);
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