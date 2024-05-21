using GrainAuger.Abstractions;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Windows;

public class SessionWindowAuger<T>(
    IAsyncObserver<List<T>> output,
    // this should be configurable
    IPersistentState<List<T>> currentWindowState,
    IAugerContext context,
    TimeSpan sessionTimeout
) : IAsyncObserver<T>
{
    private IDisposable? _timer;
    
    private async Task DumpWindow(object _)
    {
        if (currentWindowState.State.Count != 0)
        {
            await output.OnNextAsync(currentWindowState.State);
            await currentWindowState.ClearStateAsync();
        }
        
        _timer?.Dispose();
        _timer = null;
    }
    
    public async Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        _timer ??= context.RegisterTimer(DumpWindow, new object(), sessionTimeout, sessionTimeout);
        
        currentWindowState.State.Add(item);
        await currentWindowState.WriteStateAsync();
    }
    
    public Task OnCompletedAsync()
    {
        return output.OnCompletedAsync();
    }
    
    public async Task OnErrorAsync(Exception ex)
    {
        await output.OnErrorAsync(ex);
    }
}