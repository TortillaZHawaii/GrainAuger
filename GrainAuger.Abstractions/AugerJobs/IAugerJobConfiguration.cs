namespace GrainAuger.Abstractions.AugerJobs
{
    public interface IAugerJobConfiguration
    {
        void Configure(AugerJobBuilder builder);
    }
}
