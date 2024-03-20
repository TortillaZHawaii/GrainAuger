using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;

public interface IGrainContext
{
    public IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state,
        TimeSpan dueTime, TimeSpan period);
}

public class GrainContext : IGrainContext
{
    private Func<Func<object, Task>, object, TimeSpan, TimeSpan, IDisposable> _registerTimerHandle;

    public GrainContext(Func<Func<object, Task>, object, TimeSpan, TimeSpan, IDisposable> registerTimerHandle)
    {
        _registerTimerHandle = registerTimerHandle;
    }

    public IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state,
        TimeSpan dueTime, TimeSpan period) => _registerTimerHandle(asyncCallback, state, dueTime, period);
}
