using GrainAuger.Abstractions;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Detectors;

public class SmallThenLargeDetectorState
{
    public bool WasLastTransactionSmall { get; set; } = false;
}

public class SmallThenLargeDetector(
    [PersistentState("SmallThenLargeDetector", "AugerStore")] IPersistentState<SmallThenLargeDetectorState> state,
    ILogger<SmallThenLargeDetector> logger,
    IAsyncObserver<Alert> output)
    : IAsyncObserver<CardTransaction>
{
    private const int SmallAmount = 400;
    private const int LargeAmount = 500;

    public async Task OnNextAsync(CardTransaction input, StreamSequenceToken? token = null)
    {
        logger.LogInformation("WasLastTransactionSmall: {WasLastTransactionSmall}",
            state.State.WasLastTransactionSmall);
        if (state.State.WasLastTransactionSmall && input.Amount > LargeAmount)
        {
            await output.OnNextAsync(new Alert(input, "Large amount after small amount"));
        }

        if (input.Amount < SmallAmount)
        {
            logger.LogInformation("Setting WasLastTransactionSmall to true");
            state.State.WasLastTransactionSmall = true;
            await state.WriteStateAsync();
        }
        else
        {
            logger.LogInformation("Setting WasLastTransactionSmall to false");
            state.State.WasLastTransactionSmall = false;
            await state.WriteStateAsync();
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