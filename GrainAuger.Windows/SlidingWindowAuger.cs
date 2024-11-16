using GrainAuger.Abstractions;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Windows;

public class SlidingWindowAuger<T>(
    TimeSpan windowSize,
    TimeSpan windowSlide,
    IAsyncObserver<List<T>> output,
    IAugerContext context
) : IAsyncObserver<T>
{
    private IDisposable? _windowStartTimer;
    private readonly List<SlidingWindow<T>> _window = [];

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
        
        _window.Remove(window);
        window.EndTimer!.Dispose();
    }

    private Task StartNewWindow(object _)
    {
        var window = new SlidingWindow<T>();
        window.EndTimer = context.RegisterTimer(
            DumpWindow, 
            (object)window,
            windowSize + windowSlide, 
            windowSize + windowSlide);
        _window.Add(window);
        return Task.CompletedTask;
    }

    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        _windowStartTimer ??= context.RegisterTimer(
            StartNewWindow,
            new object(),
            windowSize,
            windowSize);
        
        // add item to all windows
        foreach (var window in _window)
        {
           window.Window.Add(item);
        }
        
        return Task.CompletedTask;
    }

    public async Task OnCompletedAsync()
    {
        _windowStartTimer?.Dispose();
        await Task.WhenAll(_window.Select(DumpWindow));
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
