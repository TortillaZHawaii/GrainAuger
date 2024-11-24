using GrainAuger.Abstractions;
using Orleans.Streams;

namespace GrainAuger.Windows;

public class SessionWindowAuger<T>(
    TimeSpan sessionTimeout,
    IAsyncObserver<List<T>> output,
    IAugerContext context
) : IAsyncObserver<T>
{
    private IDisposable? _timer;
    private readonly List<T> _window = [];
    
    private async Task DumpWindow(object _)
    {
        if (_window.Count != 0)
        {
            await output.OnNextAsync(_window);
            _window.Clear();
        }
        
        _timer?.Dispose();
        _timer = null;
    }
    
    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        _timer ??= context.RegisterTimer(DumpWindow, new object(), sessionTimeout, sessionTimeout);
        
        _window.Add(item);
        
        return Task.CompletedTask;
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