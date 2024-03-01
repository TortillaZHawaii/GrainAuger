namespace GrainAuger.Examples.Abstractions;

public interface IAugerGrain
{
    public Task StartAsync(string providerName, string streamNamespace, Guid inputStreamGuid,
        Guid outputStreamGuid);
    public Task StopAsync();
}
