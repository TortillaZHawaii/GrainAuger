namespace GrainAuger.Examples.Abstractions;

public interface IAugerGrain : IGrainWithStringKey
{
    public Task StartAsync(string providerName, string streamNamespace, Guid inputStreamGuid,
        Guid outputStreamGuid);
    public Task StopAsync();
}
