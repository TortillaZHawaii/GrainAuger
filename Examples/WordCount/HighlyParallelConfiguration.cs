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
        
        var lbStream = inputStream.Process<LoadBalancer1>();
        
        var keyByStream = lbStream.Process<GuidKeyBy>();
    }
}

class LoadBalancer1(IStreamProvider streamProvider, string name)
    : RoundRobinLoadBalancer<string>(name, streamProvider, 1024);

class Window1(
    IAsyncObserver<List<string>> output,
    IPersistentState<List<SlidingWindow<string>>> currentWindowsState,
    IAugerContext context) 
: SlidingWindowAuger<string>(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), output, currentWindowsState, context);

class GuidKeyBy(IStreamProvider streamProvider, string name)
    : KeyByBalancer<string, Guid>(name, streamProvider, s => Guid.NewGuid());