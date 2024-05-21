using GrainAuger.Abstractions;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Windows;

public class SlidingWindowAuger<T>(
    IAsyncObserver<List<T>> output,
    // this should be configurable
    IPersistentState<List<SlidingWindow<T>>> currentWindowsState,
    IAugerContext context
) : IAsyncObserver<T>
{
    private readonly TimeSpan _windowSize = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _windowSlide = TimeSpan.FromSeconds(30);
    
    private IDisposable? _windowStartTimer;

    private async Task DumpWindow(object obj)
    {
        var window = obj as SlidingWindow<T>;
        
        if (window == null)
        {
            return;
        }
        
        if (window.Window.Count != 0)
        {
            await output.OnNextAsync(window.Window);
        }
        
        currentWindowsState.State.Remove(window);
        window.EndTimer!.Dispose();
    }

    private Task StartNewWindow(object _)
    {
        var window = new SlidingWindow<T>();
        window.EndTimer = context.RegisterTimer(
            DumpWindow, 
            (object)window,
            _windowSize + _windowSlide, 
            _windowSize + _windowSlide);
        currentWindowsState.State.Add(window);
        return Task.CompletedTask;
    }

    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        _windowStartTimer ??= context.RegisterTimer(
            StartNewWindow,
            new object(),
            _windowSize,
            _windowSize);
        
        // add item to all windows
        foreach (var window in currentWindowsState.State)
        {
            window.Window.Add(item);
        }
        
        return Task.CompletedTask;
    }

    public async Task OnCompletedAsync()
    {
        _windowStartTimer?.Dispose();
        await Task.WhenAll(currentWindowsState.State.Select(DumpWindow));
    }

    public async Task OnErrorAsync(Exception ex)
    {
        _windowStartTimer?.Dispose();
        await output.OnErrorAsync(ex);
    }
}

public class SlidingWindow<T>
{
    public IDisposable? EndTimer { get; set; } = null;
    public List<T> Window { get; } = [];
}
