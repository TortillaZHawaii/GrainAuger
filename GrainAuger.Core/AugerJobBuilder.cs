using GrainAuger.Abstractions;

namespace GrainAuger.Core
{
    public class AugerJobBuilder : IAugerJobBuilder
    {
        public IAugerStream FromStream<TData, TKey>(string providerName, string streamName)
            where TData : notnull where TKey : notnull
        {
            return new AugerStream(providerName, streamName, typeof(TData));
        }
    }
}
