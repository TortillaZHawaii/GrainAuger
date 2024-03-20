namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;

public interface IAuger<in TIn, out TOut>
{
    public Task ProcessAsync(TIn input, Func<TOut, Task> collect);
}

public abstract class Auger<TIn, TOut> : IAuger<TIn, TOut>
{
    public abstract Task ProcessAsync(TIn input, Func<TOut, Task> collect);
}
