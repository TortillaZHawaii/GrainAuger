using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Streams;
using Orleans.TestingHost;

namespace WordCount.Test;

public class IntegrationTests
{
    private static async Task<TestCluster> SetUp()
    {
        var builder = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<SiloConfigurator>()
            .AddClientBuilderConfigurator<ClientConfigurator>();
        var cluster = builder.Build();
        await cluster.DeployAsync();
        return cluster;
    }

    private static async Task<TestCluster> SetUpDisabled()
    {
        var builder = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<SiloConfiguratorWithFlagDisabled>()
            .AddClientBuilderConfigurator<ClientConfigurator>();
        var cluster = builder.Build();
        await cluster.DeployAsync();
        return cluster;
    }

    private static async Task TearDown(TestCluster cluster)
    {
        await cluster.StopAllSilosAsync();
    }

    [TestCase(new[] { "Hello", "World" }, new[] { 1, 2 })]
    [TestCase(new[] { "Hello", "World", "this", "is", "a", "test" }, new[] { 1, 2, 3, 4, 5, 6 })]
    [TestCase(new string[0], new int[0])]
    [TestCase(new[] { "Five words in this sentence", "Then extra three" }, new[] { 5, 8 })]
    public async Task TestConsequentWordCount(string[] items, int[] expectedCounts)
    {
        // Arrange
        var cluster = await SetUp();
        var streamProvider = cluster.Client.GetStreamProvider("MemoryStream");
        var inputStream = streamProvider.GetStream<string>(StreamId.Create("WordCountInput", "abc"));
        var outputStream = streamProvider.GetStream<int>(StreamId.Create("countStream", "abc"));
        var mockReader = new MockReader<int>();
        await outputStream.SubscribeAsync(mockReader);

        foreach (var item in items)
        {
            // Act
            await inputStream.OnNextAsync(item);
        }

        await Task.Delay(400);
        
        // Assert
        Assert.That(mockReader.ReceivedItems.ToArray(), Is.EqualTo(expectedCounts));
        await TearDown(cluster);
    }

    [TestCase(new [] { "Hello", "World" }, 0)]
    [TestCase(new[] { "Hello", "World", "this", "is", "a", "test" }, 0)]
    [TestCase(new string[0], 0)]
    [TestCase(new[] { "Five words in this sentence", "Then extra three" }, 0)]
    public async Task TestWordCountWithFeatureFlagDisabled(string[] items, int size)
    {
        // Arrange
        var cluster = await SetUpDisabled();
        var streamProvider = cluster.Client.GetStreamProvider("MemoryStream");
        var inputStream = streamProvider.GetStream<string>(StreamId.Create("WordCountInput", "abc"));
        var outputStream = streamProvider.GetStream<int>(StreamId.Create("countStream", "abc"));
        var mockReader = new MockReader<int>();
        await outputStream.SubscribeAsync(mockReader);

        foreach (var item in items)
        {
            // Act
            await inputStream.OnNextAsync(item);
        }

        await Task.Delay(400);
        
        // Assert
        Assert.That(mockReader.ReceivedItems, Is.Empty);
        await TearDown(cluster);
    }
}

file class SiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        });
        hostBuilder
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryGrainStorageAsDefault()
            .AddStreaming();
        hostBuilder.AddMemoryStreams("MemoryStream");
    }
}

file class SiloConfiguratorWithFlagDisabled : ISiloConfigurator
{
    public void Configure(ISiloBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            var service = new FeatureFlagService
            {
                Enabled = false
            };
            services.AddSingleton<IFeatureFlagService>(service);
        });
        hostBuilder
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryGrainStorageAsDefault()
            .AddStreaming();
        hostBuilder.AddMemoryStreams("MemoryStream");
    }
}

file class ClientConfigurator : IClientBuilderConfigurator
{
    public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
    {
        clientBuilder.AddMemoryStreams("MemoryStream");
    }
}

file class MockReader<T> : IAsyncObserver<T>
{
    public List<T> ReceivedItems { get; } = [];
    
    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        ReceivedItems.Add(item);
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