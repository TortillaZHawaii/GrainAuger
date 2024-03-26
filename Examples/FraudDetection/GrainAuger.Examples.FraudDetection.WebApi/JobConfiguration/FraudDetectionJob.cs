using GrainAuger.Abstractions.AugerJobs;
using GrainAuger.Examples.FraudDetection.WebApi.Detectors;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class FraudDetectionJob : IAugerJobConfiguration
{
    public void Configure(AugerJobBuilder builder)
    {
        var inputStream = builder.FromStream("AugerStreamProvider", "input");
        
        var overLimitStream = inputStream.Process<OverLimitDetector>("overLimitStream");
        var expiredCardStream = inputStream.Process<ExpiredCardDetector>("expiredCardStream");
        var normalDistributionStream = inputStream.Process<NormalDistributionDetector>("normalDistributionStream");
        var smallThenLargeStream = inputStream.Process<SmallThenLargeDetector>("smallThenLargeStream");
    }
}