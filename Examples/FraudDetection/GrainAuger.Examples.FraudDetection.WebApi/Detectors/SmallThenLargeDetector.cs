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
    private const int SmallAmount = 400;
    private const int LargeAmount = 500;

    private readonly ILogger<SmallThenLargeDetector> _logger;
    private readonly IPersistentState<SmallThenLargeDetectorState> _state;
    private IDisposable? _timer;

    public SmallThenLargeDetector(
        [PersistentState("SmallThenLargeDetector", "AugerStore")]
        IPersistentState<SmallThenLargeDetectorState> state, ILogger<SmallThenLargeDetector> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override async Task ProcessAsync(CardTransaction input, Func<Alert, Task> collect)
    {
        _timer?.Dispose();

        _logger.LogInformation("WasLastTransactionSmall: {WasLastTransactionSmall}",
            _state.State.WasLastTransactionSmall);
        if (_state.State.WasLastTransactionSmall && input.Amount > LargeAmount)
        {
            await collect(new Alert(input, "Large amount after small amount"));
        }

        if (input.Amount < SmallAmount)
        {
            _logger.LogInformation("Setting WasLastTransactionSmall to true");
            _state.State.WasLastTransactionSmall = true;
            await _state.WriteStateAsync();
            _logger.LogInformation("Registered timer");
            _timer = RegisterTimer(ClearState, new object(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
    }

    private async Task ClearState(object _)
    {
        _logger.LogInformation("Clearing state");
        await _state.ClearStateAsync();
        _timer?.Dispose();
    }
}