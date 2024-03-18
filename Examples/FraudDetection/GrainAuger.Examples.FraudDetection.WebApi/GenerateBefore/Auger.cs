namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;

public interface IAuger<in TIn, out TOut>
{
    public Task ProcessAsync(TIn input, Func<TOut, Task> collect);
}

public abstract class Auger<TIn, TOut> : IAuger<TIn, TOut>
{
    public abstract Task ProcessAsync(TIn input, Func<TOut, Task> collect);

    // this should be injected by grain, and only be visible by grain, not auger itself
    internal Func<Func<object, Task>, object, TimeSpan, TimeSpan, IDisposable> RegisterTimerHandle = null!;

    protected IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state,
        TimeSpan dueTime, TimeSpan period) => RegisterTimerHandle(asyncCallback, state, dueTime, period);
}
