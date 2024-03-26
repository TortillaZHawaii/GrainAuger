namespace GrainAuger.Abstractions
{
    public interface IAugerJobConfiguration
    {
        void Configure(IAugerJobBuilder builder);
    }
}
