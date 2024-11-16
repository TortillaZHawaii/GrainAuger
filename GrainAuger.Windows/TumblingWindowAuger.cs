using GrainAuger.Abstractions;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Windows;

public class TumblingWindowAuger<T>(
    TimeSpan windowSize,
    IAsyncObserver<List<T>> output, 
    IAugerContext context
    ) : IAsyncObserver<T>
{
    private readonly List<T> _window = [];
    
    // potential memory leak if not disposed ?
    private IDisposable? _timer;
    
    private async Task DumpWindow(object _)
    {
        if (_window.Count == 0)
        {
            // No data windows are not send further
            return;
        }

        await output.OnNextAsync(_window);
        _window.Clear();
    }
    
    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        _timer ??= context.RegisterTimer(DumpWindow, new object(), windowSize, windowSize);
        
        _window.Add(item);
        
        return Task.CompletedTask;
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