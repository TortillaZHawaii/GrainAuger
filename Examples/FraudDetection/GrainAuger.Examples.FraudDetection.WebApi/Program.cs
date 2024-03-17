using GrainAuger.Examples.FraudDetection.WebApi.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .AddMemoryGrainStorage("PubSubStore")
        .AddMemoryGrainStorage("AugerStore")
        .AddMemoryGrainStorageAsDefault();

    siloBuilder.UseLocalhostClustering();

    siloBuilder.AddMemoryStreams("Kafka");

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

app.MapGet("/start", (IGrainFactory grainFactory) =>
    {
        var producer = grainFactory.GetGrain<IProducerGrain>("1");
        producer.StartAsync();
    })
    .WithName("StartProducing")
    .WithOpenApi();

app.MapGet("/stop", (IGrainFactory grainFactory) =>
    {
        var producer = grainFactory.GetGrain<IProducerGrain>("1");
        producer.StopAsync();
    })
    .WithName("StopProducing")
    .WithOpenApi();

app.Run();
