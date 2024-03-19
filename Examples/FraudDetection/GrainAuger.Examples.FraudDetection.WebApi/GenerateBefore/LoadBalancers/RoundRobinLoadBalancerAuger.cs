using Orleans.Runtime;

namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore.LoadBalancers;

public class RoundRobinLoadBalancerAuger<T> : Auger<T, T>
{
    private readonly Guid[] _pool;
    private int _index = 0;
    
    public RoundRobinLoadBalancerAuger(int poolSize)
    {
        _pool = new Guid[poolSize];
        for (var i = 0; i < poolSize; i++)
        {
            _pool[i] = Int2Guid(i);
        }
    }
    
    public override async Task ProcessAsync(T input, Func<T, Task> collect)
    {
        // we need to change the output stream depending on the pool
        var guid = _pool[_index];
        _index = _index + 1 >= _pool.Length ? 0 : _index + 1;

        var outputStreamId = StreamId.Create("GrainAuger_XXX", guid);
        // we would need to punch a hole in the grain to get the stream provider
        // and use output stream instead of collect
        var outputStream = GetStreamProvider("AugerStreamProvider").GetStream<T>(outputStreamId);
        await outputStream.OnNextAsync(input);
    }
    
    private static Guid Int2Guid(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}