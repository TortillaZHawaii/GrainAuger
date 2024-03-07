using GrainAuger.Examples.Grains;
using Orleans.Streams.Kafka.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string providerName = "StreamProvider";
string streamNamespace = "Auger";

var membershipConnectionString = builder.Configuration.GetConnectionString("membership");
var storageConnectionString = builder.Configuration.GetConnectionString("storage");
var messagingConnectionString = builder.Configuration.GetConnectionString("messaging");

var grainStorageRedisOptions = new StackExchange.Redis.ConfigurationOptions
{
    EndPoints = {storageConnectionString ?? ""}
};

builder.Host.UseOrleans(siloBuilder =>
{
    if (membershipConnectionString != null)
    {
        siloBuilder.UseRedisClustering(membershipConnectionString);
    }
    else
    {
        siloBuilder.UseLocalhostClustering();
    }

    if (storageConnectionString != null)
    {
        siloBuilder
            .AddRedisGrainStorage("Auger", options =>
                options.ConfigurationOptions = grainStorageRedisOptions)
            .AddRedisGrainStorage("PubSubStore", options =>
                options.ConfigurationOptions = grainStorageRedisOptions);
    }
    else
    {
        siloBuilder
            .AddMemoryGrainStorage("Auger")
            .AddMemoryGrainStorage("PubSubStore");
    }
    
    if (messagingConnectionString != null)
    {
        siloBuilder.AddKafka(providerName)
            .WithOptions(options =>
            {
                options.BrokerList = new[] { messagingConnectionString };
                options.ConsumerGroupId = streamNamespace;
                options.ConsumeMode = ConsumeMode.StreamEnd;

                options
                    .AddTopic(streamNamespace, new TopicCreationConfig
                    {
                        AutoCreate = true, Partitions = 2, ReplicationFactor = 1
                    });
            })
            .AddJson()
            .AddLoggingTracker();
    }
    else
    {
        siloBuilder.AddMemoryStreams(providerName);
    }
    
    siloBuilder.UseDashboard(x => x.HostSelf = true);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Map("/dashboard", configuration => { configuration.UseOrleansDashboard(); });

Guid producerStreamGuid = Guid.NewGuid();
Guid selectorStreamGuid = Guid.NewGuid();
Guid whereStreamGuid = Guid.NewGuid();
Guid distinctStreamGuid = Guid.NewGuid();
Guid chunkStreamGuid = Guid.NewGuid();

string selectorKey = "Selector";
string whereKey = "Where";
string producerKey = "Producer";
string distinctKey = "Distinct";
string chunkKey = "Chunk";


app.MapGet("/start", async (IGrainFactory factory) =>
{
    var producerGrain = factory.GetGrain<IProducerGrain>(producerKey);
    await producerGrain.StartAsync(providerName, streamNamespace, producerStreamGuid);
    
    // split the stream into two streams
    // stream for distinct
    var distinctAugerGrain = factory.GetGrain<IDistinctAuger<int>>(distinctKey);
    await distinctAugerGrain.StartAsync(providerName, streamNamespace,
        producerStreamGuid, distinctStreamGuid);
    
    // stream for select, where, chunk
    var selectAugerGrain = factory.GetGrain<ISelectAuger<int, int>>(selectorKey);
    await selectAugerGrain.StartAsync(providerName, streamNamespace,
        producerStreamGuid, selectorStreamGuid);
    
    var whereAugerGrain = factory.GetGrain<IWhereAuger<int>>(whereKey);
    await whereAugerGrain.StartAsync(providerName, streamNamespace,
        selectorStreamGuid, whereStreamGuid);

    var chunkAugerGrain = factory.GetGrain<IChunkAuger<int>>(chunkKey);
    await chunkAugerGrain.StartAsync(providerName, streamNamespace,
        whereStreamGuid, chunkStreamGuid);

    return "Started";
});

app.MapGet("/stop", async (IGrainFactory factory) =>
{
    var producerGrain = factory.GetGrain<IProducerGrain>(producerKey);
    await producerGrain.StopAsync();
    
    var distinctAugerGrain = factory.GetGrain<IDistinctAuger<int>>(distinctKey);
    await distinctAugerGrain.StopAsync();

    var selectAugerGrain = factory.GetGrain<ISelectAuger<int, int>>(selectorKey);
    await selectAugerGrain.StopAsync();
    
    var whereAugerGrain = factory.GetGrain<IWhereAuger<int>>(whereKey);
    await whereAugerGrain.StopAsync();
    
    var chunkAugerGrain = factory.GetGrain<IChunkAuger<int>>(chunkKey);
    await chunkAugerGrain.StopAsync();

    return "Stopped";
});

app.MapPost("/counter", async (IGrainFactory factory) =>
{
    
    var grain = factory.GetGrain<ICounterJournaledGrain>("");
    return "Incremented";
});

app.Run();

app.Run();
