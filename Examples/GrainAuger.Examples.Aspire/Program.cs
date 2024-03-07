var builder = DistributedApplication.CreateBuilder(args);

builder.AddKafka("messaging");
builder.AddRedis("membership");

builder.Build().Run();