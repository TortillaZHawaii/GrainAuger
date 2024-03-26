namespace GrainAuger.Abstractions
{
    internal class AugerStream : IAugerStream
    {
        public AugerStream(string providerName, string name)
        {
            ProviderName = providerName;
            Name = name;
        }

        public string Name { get; }
        public string ProviderName { get; }
        
        public IAugerStream Process<T1>(string name)
        {
            return new AugerStream(ProviderName, name);
        }

        public IAugerStream Process<T1, T2>(string name)
        {
            return new AugerStream(ProviderName, name);
        }

        public IAugerStream Process<T1, T2, T3>(string name)
        {
            return new AugerStream(ProviderName, name);
        }

        public IAugerStream Process<T1, T2, T3, T4>(string name)
        {
            return new AugerStream(ProviderName, name);
        }
    }
}
