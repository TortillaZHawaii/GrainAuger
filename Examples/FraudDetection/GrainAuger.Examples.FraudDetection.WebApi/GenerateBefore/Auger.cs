namespace GrainAuger.Examples.FraudDetection.WebApi.Grains;

public abstract class Auger<TIn, TOut>
{
    public abstract Task ProcessAsync(TIn input, Func<TOut, Task> collect);
}
