using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class OverLimitDetector(IAsyncObserver<Alert> output) : IAsyncObserver<CardTransaction>
{
    public async Task OnNextAsync(CardTransaction input, StreamSequenceToken? token = null)
    {
        if (input.Amount > input.LimitLeft)
        {
            var alert = new Alert(input, "Over limit");
            await output.OnNextAsync(alert);
        }
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
