using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

string redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "redis";
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();
