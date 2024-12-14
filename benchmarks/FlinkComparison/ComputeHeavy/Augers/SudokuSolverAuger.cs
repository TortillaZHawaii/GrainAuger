using Orleans.Streams;

namespace ComputeHeavy.Augers;

public class SudokuSolverAuger(IAsyncObserver<byte[,]> output) : IAsyncObserver<byte[,]>
{
    public async Task OnNextAsync(byte[,] board, StreamSequenceToken? token = null)
    {
        
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex) => Task.CompletedTask;
}

