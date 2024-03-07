using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafka("messaging");
var membership = builder.AddRedis("membership");
var storage = builder.AddRedis("storage");

builder.AddProject<GrainAuger_Examples_WebApi>("silo1")
    // .WithReference(messaging)
    .WithReference(membership)
    .WithReference(storage);
    
builder.Build().Run();
