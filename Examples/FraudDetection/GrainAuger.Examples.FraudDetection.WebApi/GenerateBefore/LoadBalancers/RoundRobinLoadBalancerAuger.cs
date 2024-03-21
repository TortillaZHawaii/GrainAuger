using Orleans.Runtime;

namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore.LoadBalancers;

public class RoundRobinLoadBalancerAuger<T> : Auger<T, T>
{
    public int PoolSize { get; }
    private int _index = 0;
    
    public RoundRobinLoadBalancerAuger(int poolSize)
    {
        PoolSize = poolSize;
    }
    
    public override async Task ProcessAsync(T input, Func<T, Task> collect)
    {
        // we need to change the output stream depending on the pool
        _index = _index + 1 >= PoolSize ? 0 : _index + 1;
        throw new NotImplementedException("This is a placeholder for the actual implementation");
        var outputStreamId = StreamId.Create("GrainAuger_XXX", _index);
        // we would need to punch a hole in the grain to get the stream provider
        // and use output stream instead of collect
        //var outputStream = GetStreamProvider("AugerStreamProvider").GetStream<T>(outputStreamId);
        //await outputStream.OnNextAsync(input);
    }
}