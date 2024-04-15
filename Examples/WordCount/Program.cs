using Orleans.Runtime;
using Orleans.Streams;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.Map("/dashboard", configuration =>
{
    configuration.UseOrleansDashboard();
});

app.Map("/count", async (IStreamProvider streamProvider, string text) =>
{
    var streamId = StreamId.Create("WordCountInput", "abc");
    var stream = streamProvider.GetStream<string>(streamId);

    await stream.OnNextAsync(text);
});

app.Run();
