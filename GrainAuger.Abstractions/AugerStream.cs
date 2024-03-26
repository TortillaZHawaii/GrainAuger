namespace GrainAuger.Abstractions
{
    internal class AugerStream : IAugerStream
    {
        public IAugerStream Process<T1>()
        {
            return new AugerStream();
        }

        public IAugerStream Process<T1, T2>()
        {
            return new AugerStream();
        }

        public IAugerStream Process<T1, T2, T3>()
        {
            return new AugerStream();
        }

        public IAugerStream Process<T1, T2, T3, T4>()
        {
            return new AugerStream();
        }
    }
}
