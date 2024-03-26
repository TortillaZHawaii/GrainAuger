using GrainAuger.Abstractions.AugerJobs;
using GrainAuger.Examples.FraudDetection.WebApi.Detectors;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class FraudDetectionJob : IAugerJobConfiguration
{
    public void Configure(AugerJobBuilder builder)
    {
        var inputStream = builder.FromStream("AugerStreamProvider", "input");
        
        var overLimitStream = inputStream.Process<OverLimitDetector>();
        var expiredCardStream = inputStream.Process<ExpiredCardDetector>();
        var normalDistributionStream = inputStream.Process<NormalDistributionDetector>();
        var smallThenLargeStream = inputStream.Process<SmallThenLargeDetector>();
    }
}