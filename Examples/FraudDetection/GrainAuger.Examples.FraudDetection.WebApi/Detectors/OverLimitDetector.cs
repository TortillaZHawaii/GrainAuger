using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class OverLimitDetector : Auger<CardTransaction, Alert>
{
    public override async Task ProcessAsync(CardTransaction input, Func<Alert, Task> collect)
    {
        if (input.Amount > input.LimitLeft)
        {
            var alert = new Alert(input, "Over limit");
            await collect(alert);
        }
    }
}
