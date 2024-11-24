using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class QuickTravelDetector(IAsyncObserver<Alert> output) : IAsyncObserver<List<CardTransaction>>
{
    public async Task OnNextAsync(List<CardTransaction> item, StreamSequenceToken? token = null)
    {
        var averageLatitude = item.Average(x => x.Latitude);
        var averageLongitude = item.Average(x => x.Longitude);
        var radius = 0.1;
        
        foreach (var transaction in item)
        {
            var distance = Math.Sqrt(Math.Pow(transaction.Latitude - averageLatitude, 2) + Math.Pow(transaction.Longitude - averageLongitude, 2));
            if (distance > radius)
            {
                var alert = new Alert(transaction, "Quick travel");
                await output.OnNextAsync(alert);
                break;
            }
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