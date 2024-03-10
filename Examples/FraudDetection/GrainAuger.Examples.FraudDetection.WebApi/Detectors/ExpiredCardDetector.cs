using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using GrainAuger.Examples.FraudDetection.WebApi.Grains;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class ExpiredCardDetector : Auger<CardTransaction, Alert>
{
    public override async Task ProcessAsync(CardTransaction input, Func<Alert, Task> collect)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var expirationDate = new DateOnly(input.Card.ExpiryYear, input.Card.ExpiryMonth, 1).AddMonths(1);

        if (expirationDate < now)
        {
            var alert = new Alert(input, "Expired card");
            await collect(alert);
        }
    }
}
