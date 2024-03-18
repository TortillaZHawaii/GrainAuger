using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;
using Orleans.Runtime;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class SmallThenLargeDetectorState
{
    public bool WasLastTransactionSmall { get; set; } = false;
}

public class SmallThenLargeDetector : Auger<CardTransaction, Alert>
{
    private const int SmallAmount = 20;
    private const int LargeAmount = 500;

    private readonly IPersistentState<SmallThenLargeDetectorState> _state;
    private IDisposable? _timer;

    public SmallThenLargeDetector(
        [PersistentState("SmallThenLargeDetector", "AugerStore")]
        IPersistentState<SmallThenLargeDetectorState> state)
    {
        _state = state;
    }

    public override async Task ProcessAsync(CardTransaction input, Func<Alert, Task> collect)
    {
        _timer?.Dispose();

        if (_state.State.WasLastTransactionSmall && input.Amount > LargeAmount)
        {
            await collect(new Alert(input, "Large amount after small amount"));
        }

        if (input.Amount < SmallAmount)
        {
            _state.State.WasLastTransactionSmall = true;
            await _state.WriteStateAsync();
            _timer = RegisterTimer(ClearState, new object(), TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
    }

    private async Task ClearState(object _)
    {
        await _state.ClearStateAsync();
        _timer?.Dispose();
    }
}