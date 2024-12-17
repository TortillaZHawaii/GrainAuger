using CloudNative.Workload;
using CloudNative.Workload.Grains;
using Microsoft.AspNetCore.Mvc;
using Orleans.Streams.Kafka.Config;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

string redisConnection = builder.Configuration["RedisConnection"] ?? "";
string kafkaConnection = builder.Configuration["KafkaConnection"] ?? "";

var redisConfiguration = new ConfigurationOptions
{
    EndPoints = { redisConnection }
};

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddMemoryGrainStorage("PubSubStore");
    siloBuilder.AddMemoryStreams("MemoryProvider");

    siloBuilder.UseKubernetesHosting();

    siloBuilder.AddKafkaStreamProvider("Kafka", options => 
    {
        options.BrokerList = [kafkaConnection];
        options.ConsumerGroupId = "GrainAuger";
        var config = new TopicCreationConfig
        {
            AutoCreate = true,
            Partitions = 10,
        };
        // options.AddExternalTopic<string>("inputTransactions");
        options.AddTopic("inputStream", config);
        options.AddTopic("ioBoundStream", config);
        options.AddTopic("cpuBoundStream", config);
        options.AddTopic("baseStream", config);
    });

    siloBuilder.AddRedisGrainStorage("RedisStore", options => { options.ConfigurationOptions = redisConfiguration; });
    siloBuilder.UseRedisClustering(options => { options.ConfigurationOptions = redisConfiguration; });

    siloBuilder.UseDashboard(x => x.HostSelf = true);
});

builder.Services.AddHostedService<KafkaSource>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaSource>>();
    var clusterClient = sp.GetRequiredService<IClusterClient>();
    return new KafkaSource(logger, clusterClient, kafkaConnection, "inputTransactions");
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

app.MapGet("/print", async (IClusterClient client, ILogger<CardTransaction> log, string text) =>
{
    log.LogInformation("Printing {Text}", text);
    var streamProvider = client.GetStreamProvider("Kafka");
    log.LogInformation("Stream provider");
    var streamId = StreamId.Create("inputStream", text);
    var stream = streamProvider.GetStream<string>(streamId);
    log.LogInformation("Stream");
    try
    {
        await stream.OnNextAsync(text);
    } catch (Exception ex)
    {
        log.LogError("Error processing item: {Error}", ex.Message);
        return "Error";
    }
    log.LogInformation("Streamed");
    return "Printed";
});

app.MapPost("/transaction", async (IClusterClient client, ILogger<CardTransaction> log, CardTransaction transaction) =>
{
    log.LogInformation("Transaction: {Transaction}", transaction);
    var streamProvider = client.GetStreamProvider("Kafka");
    log.LogInformation("Stream provider");
    var streamId = StreamId.Create("inputStream", transaction.CardNumber);
    var stream = streamProvider.GetStream<CardTransaction>(streamId);
    log.LogInformation("Stream");
    try
    {
        await stream.OnNextAsync(transaction);
    } catch (Exception ex)
    {
        log.LogError("Error processing item: {Error}", ex.Message);
        return "Error";
    }
    log.LogInformation("Streamed");
    return "Processed";
});

app.MapGet("/version", () => "0.2.0").WithName("Version");

// Kubernetes probes
app.MapGet("/health", () => "Healthy").WithName("Health");
app.MapGet("/ready", () => "Ready").WithName("Ready");

app.Run();
