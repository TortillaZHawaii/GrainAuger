using GrainAuger.Examples.Grains;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string providerName = "StreamProvider";

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorageAsDefault();
    siloBuilder
        .AddMemoryGrainStorage("Auger");
    siloBuilder
        .AddMemoryGrainStorage("PubSubStore")
        .AddMemoryStreams(providerName);
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

string streamNamespace = "Auger";

app.MapGet("/start", async (IGrainFactory factory) =>
{
    var producerGrain = factory.GetGrain<IProducerGrain>(producerKey);
    await producerGrain.StartAsync(providerName, streamNamespace, producerStreamGuid);
    
    var distinctAugerGrain = factory.GetGrain<IDistinctAuger<int>>(distinctKey);
    await distinctAugerGrain.StartAsync(providerName, streamNamespace,
        producerStreamGuid, distinctStreamGuid);

    var selectAugerGrain = factory.GetGrain<ISelectAuger<int, int>>(selectorKey);
    await selectAugerGrain.StartAsync(providerName, streamNamespace,
        distinctStreamGuid, selectorStreamGuid);
    
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
