using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Benchmarks.ProcessChains;

public class ProcessChainsBenchmark
{
    [Params(100, 200, 500, 1000, 2000, 5000, 10000)]
    public int N { get; set; }
    
    private IHost Host { get; set; } = null!;
    
    [GlobalSetup]
    public async Task Setup()
    {
        Host = new HostBuilder()
            .UseOrleans(builder =>
            {
                builder.UseLocalhostClustering();
                builder.AddMemoryGrainStorage("PubSubStore");
                builder.AddMemoryStreams("Memory");
            })
            .Build();
        
        await Host.StartAsync();
    }
    
    [GlobalCleanup]
    public async Task Cleanup()
    {
        await Host.StopAsync();
        Host.Dispose();
    }

    [Benchmark]
    public async Task ProcessSplit()
    {
        int key = 0;
        var clusterClient = Host.Services.GetRequiredService<IClusterClient>();
        var streamProvider = clusterClient.GetStreamProvider("Memory");
        var inputStream = streamProvider.GetStream<int>(StreamId.Create("InputSplit", key));
        var outputStream = streamProvider.GetStream<int>(StreamId.Create("Split2", key));

        var waiter = new Waiter(N);
        
        await outputStream.SubscribeAsync(waiter);
        
        for (int i = 1; i <= N; i++)
        {
            await inputStream.OnNextAsync(i);
        }
        
        await waiter.Semaphore.WaitAsync();
    }
    
    [Benchmark]
    public async Task ProcessJoin()
    {
        int key = 0;
        var clusterClient = Host.Services.GetRequiredService<IClusterClient>();
        var streamProvider = clusterClient.GetStreamProvider("Memory");
        var inputStream = streamProvider.GetStream<int>(StreamId.Create("InputJoin", key));
        var outputStream = streamProvider.GetStream<int>(StreamId.Create("Joined", key));

        var waiter = new Waiter(N);
        
        await outputStream.SubscribeAsync(waiter);
        
        for (int i = 1; i <= N; i++)
        {
            await inputStream.OnNextAsync(i);
        }
        
        await waiter.Semaphore.WaitAsync();
    }

    class Waiter(int N) : IAsyncObserver<int>
    {
        public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(0);
        
        public Task OnNextAsync(int item, StreamSequenceToken? token = null)
        {
            if (item == N)
            {
                Semaphore.Release();
            }
            return Task.CompletedTask;
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
}