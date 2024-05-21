using GrainAuger.Abstractions;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Windows;

public class TumblingWindowAuger<T>(
    IAsyncObserver<List<T>> output, 
    IPersistentState<List<T>> currentWindowState,
    IAugerContext context
    ) : IAsyncObserver<T>
{
    private readonly TimeSpan _windowSize = TimeSpan.FromMinutes(1);
    
    // potential memory leak if not disposed ?
    private IDisposable? _timer;
    
    private async Task DumpWindow(object _)
    {
        if (currentWindowState.State.Count == 0)
        {
            // No data windows are not send further
            return;
        }

        await output.OnNextAsync(currentWindowState.State);
        await currentWindowState.ClearStateAsync();
    }
    
    public async Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        _timer ??= context.RegisterTimer(DumpWindow, new object(), _windowSize, _windowSize);
        
        currentWindowState.State.Add(item);
        await currentWindowState.WriteStateAsync();
    }

    public Task OnCompletedAsync()
    {
        _timer?.Dispose();
        return output.OnCompletedAsync();
    }

    public async Task OnErrorAsync(Exception ex)
    {
        _timer?.Dispose();
        await output.OnErrorAsync(ex);
    }
}