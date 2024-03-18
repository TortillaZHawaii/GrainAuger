using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;
using Orleans.Runtime;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class NormalDistributionState
{
    public ulong Count { get; set; }
    public double MVariance { get; set; }
    public double Average { get; set; }
}

public class NormalDistributionDetector : Auger<CardTransaction, Alert>
{
    private readonly IPersistentState<NormalDistributionState> _state;
    private const int CountCutOff = 10;
    private const int ZScoreCutOff = 3;
    
    public NormalDistributionDetector(
        [PersistentState("NormalDistributionDetector", "AugerStore")]
        IPersistentState<NormalDistributionState> state
    )
    {
        _state = state;
    }
    
    public override async Task ProcessAsync(CardTransaction input, Func<Alert, Task> collect)
    {
        _state.State.Count++;
        
        double previousAverage = _state.State.Average;
        _state.State.Average = previousAverage + (input.Amount - previousAverage) / _state.State.Count;
        
        _state.State.MVariance += (input.Amount - previousAverage) * (input.Amount - _state.State.Average);

        await _state.WriteStateAsync();
        
        if (_state.State.Count > CountCutOff)
        {
            var variance = _state.State.MVariance / (_state.State.Count - 1);
            var standardDeviation = Math.Sqrt(variance);
            var zScore = (input.Amount - _state.State.Average) / standardDeviation;

            if (zScore > ZScoreCutOff)
            {
                var alert = new Alert(input, "Normal distribution anomaly detected");
                await collect(alert);
            }
        }
    }
}