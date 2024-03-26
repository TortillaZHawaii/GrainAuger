using GrainAuger.Abstractions;

namespace GrainAuger.Core
{
    public class AugerJobBuilder : IAugerJobBuilder
    {
        public IAugerStream FromStream<T>(string providerName, string streamName)
        {
            return new AugerStream(providerName, streamName, typeof(T));
        }
    }
}
