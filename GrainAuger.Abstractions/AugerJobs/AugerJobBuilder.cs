namespace GrainAuger.Abstractions.AugerJobs
{
    public class AugerJobBuilder
    {
        public IAugerStream FromStream<T>(string providerName, string streamName)
        {
            return new AugerStream(providerName, streamName, typeof(T));
        }
    }
}
