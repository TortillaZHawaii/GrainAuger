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

string selectorKey = "Selector";
string whereKey = "Where";
string producerKey = "Producer";

string streamNamespace = "Auger";

app.MapGet("/start", async (IGrainFactory factory) =>
{
    var producerGrain = factory.GetGrain<IProducerGrain>(producerKey);
    await producerGrain.StartAsync(providerName, streamNamespace, producerStreamGuid);

    var selectAugerGrain = factory.GetGrain<ISelectAuger<int, int>>(selectorKey);
    await selectAugerGrain.StartAsync(providerName, streamNamespace,
        producerStreamGuid, selectorStreamGuid);
    
    var whereAugerGrain = factory.GetGrain<IWhereAuger<int>>(whereKey);
    await whereAugerGrain.StartAsync(providerName, streamNamespace,
        selectorStreamGuid, whereStreamGuid);

    return "Started";
});

app.MapGet("/stop", async (IGrainFactory factory) =>
{
    var producerGrain = factory.GetGrain<IProducerGrain>(producerKey);
    await producerGrain.StopAsync();

    var selectAugerGrain = factory.GetGrain<ISelectAuger<int, int>>(selectorKey);
    await selectAugerGrain.StopAsync();
    
    var whereAugerGrain = factory.GetGrain<IWhereAuger<int>>(whereKey);
    await whereAugerGrain.StopAsync();

    return "Stopped";
});

app.Run();

app.Run();
