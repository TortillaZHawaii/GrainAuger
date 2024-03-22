using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class FraudDetectionJob : IAugerJobConfiguration
{
    public void Configure(AugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<CardTransaction>("AugerStreamProvider", "input");
        
        var overLimitStream = inputStream.Process<OverLimitDetector, Alert>();
        var expiredCardStream = inputStream.Process<ExpiredCardDetector, Alert>();
        var normalDistributionStream = inputStream.Process<NormalDistributionDetector, Alert>();
        var smallThenLargeStream = inputStream.Process<SmallThenLargeDetector, Alert>();
        
    }
}