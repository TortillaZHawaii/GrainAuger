using GrainAuger.Abstractions;
using GrainAuger.LoadBalancers;
using GrainAuger.Windows;
using Orleans.Runtime;
using Orleans.Streams;

namespace WordCount;

abstract class HighlyParallelConfiguration
{
    [AugerJobConfiguration("ScatherGatherJob")]
    static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<string, string>("MemoryStream", "WordCountInput");

        var windowedStream = inputStream.Process<Window1>();
        //
        var lbStream = inputStream.Process<LoadBalancer1>();
        var intStream = lbStream.Process<IntProcessor>();
        //
        // var keyByStream = inputStream.Process<GuidKeyBy>();
    }
}

public class LoadBalancer1(IStreamProvider streamProvider, string name)
    : RoundRobinLoadBalancer<string>(name, streamProvider, 1024);

public class Window1(
    IAsyncObserver<List<string>> output,
    IAugerContext context) 
: SlidingWindowAuger<string>(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), output, context);

public class GuidKeyBy(IStreamProvider streamProvider, string name)
    : KeyByBalancer<string, Guid>(name, streamProvider, s => Guid.NewGuid());
    
public class IntProcessor(IAsyncObserver<string> output, ILogger<IntProcessor> logger) : IAsyncObserver<string>
{
    public async Task OnNextAsync(string item, StreamSequenceToken? token = null)
    {
        logger.LogInformation($"Processing {item}");
        await output.OnNextAsync(item);
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }
}
    