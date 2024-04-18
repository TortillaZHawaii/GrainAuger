using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class NormalDistributionState
{
    public ulong Count { get; set; }
    public double MVariance { get; set; }
    public double Average { get; set; }
}

public class NormalDistributionDetector(
    [PersistentState("NormalDistributionDetector", "AugerStore")] IPersistentState<NormalDistributionState> state,
    IAsyncObserver<Alert> output)
    : IAsyncObserver<CardTransaction>
{
    private const int CountCutOff = 10;
    private const int ZScoreCutOff = 3;

    public async Task OnNextAsync(CardTransaction input, StreamSequenceToken? token = null)
    {
        state.State.Count++;
        
        double previousAverage = state.State.Average;
        state.State.Average = previousAverage + (input.Amount - previousAverage) / state.State.Count;
        
        state.State.MVariance += (input.Amount - previousAverage) * (input.Amount - state.State.Average);

        await state.WriteStateAsync();
        
        if (state.State.Count > CountCutOff)
        {
            var variance = state.State.MVariance / (state.State.Count - 1);
            var standardDeviation = Math.Sqrt(variance);
            var zScore = (input.Amount - state.State.Average) / standardDeviation;

            if (zScore > ZScoreCutOff)
            {
                var alert = new Alert(input, "Normal distribution anomaly detected");
                await output.OnNextAsync(alert);
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