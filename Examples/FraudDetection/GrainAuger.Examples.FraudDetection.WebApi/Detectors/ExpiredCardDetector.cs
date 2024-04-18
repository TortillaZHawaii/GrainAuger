using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class ExpiredCardDetector(IAsyncObserver<Alert> output) : IAsyncObserver<CardTransaction>
{
    public async Task OnNextAsync(CardTransaction input, StreamSequenceToken? token = null)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var expirationDate = new DateOnly(input.Card.ExpiryYear, input.Card.ExpiryMonth, 1).AddMonths(1);

        if (expirationDate < now)
        {
            var alert = new Alert(input, "Expired card");
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
