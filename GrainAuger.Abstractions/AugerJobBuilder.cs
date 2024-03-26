namespace GrainAuger.Abstractions
{
    public class AugerJobBuilder
    {
        public IAugerStream FromStream(string providerName, string streamName)
        {
            return new AugerStream();
        }
    }
}
