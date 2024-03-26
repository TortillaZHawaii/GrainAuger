namespace GrainAuger.Abstractions
{
    public interface IAugerJobBuilder
    {
        IAugerStream FromStream<T>(string providerName, string streamName);
    }
}