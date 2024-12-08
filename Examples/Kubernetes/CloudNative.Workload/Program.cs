using CloudNative.Workload.Grains;
using Confluent.Kafka;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string redisConnection = builder.Configuration["RedisConnection"] ?? "redis";

var redisConfiguration = new ConfigurationOptions
{
    EndPoints = { redisConnection }
};

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddMemoryGrainStorage("PubSubStore");
    siloBuilder.AddMemoryStreams("MemoryProvider");
    
    siloBuilder.UseKubernetesHosting();

    siloBuilder.AddRedisGrainStorage("RedisStore", options => { options.ConfigurationOptions = redisConfiguration; });
    siloBuilder.UseRedisClustering(options => { options.ConfigurationOptions = redisConfiguration; });

    siloBuilder.UseDashboard(x => x.HostSelf = true);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.Map("/dashboard", configuration => { configuration.UseOrleansDashboard(); });

app.MapGet("/hello", async (IClusterClient client, string text) =>
{
    var grain = client.GetGrain<IHelloGrain>(text);
    return await grain.SayHello();
});

// Kubernetes probes
app.MapGet("/health", () => "Healthy").WithName("Health");
app.MapGet("/ready", () => "Ready").WithName("Ready");

app.Run();
