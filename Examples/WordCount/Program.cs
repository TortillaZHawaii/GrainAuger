using Orleans.Runtime;
using WordCount;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IFeatureFlagService, FeatureFlagService>();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .AddMemoryGrainStorage("PubSubStore")
        .AddMemoryGrainStorageAsDefault();

    siloBuilder.UseLocalhostClustering();

    siloBuilder.AddMemoryStreams("MemoryStream");

    siloBuilder.UseDashboard(x => x.HostSelf = true);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Map("/dashboard", configuration =>
{
    configuration.UseOrleansDashboard();
});

app.MapPost("/count", async (IClusterClient client, string text) =>
    {
        var streamProvider = client.GetStreamProvider("MemoryStream");
        var streamId = StreamId.Create("WordCountInput", "abc");
        var stream = streamProvider.GetStream<string>(streamId);

        await stream.OnNextAsync(text);
    })
    .WithName("CountWords")
    .WithOpenApi();

app.Run();
