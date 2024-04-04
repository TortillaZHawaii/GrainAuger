using GrainAuger.Abstractions;
using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class FraudDetectionJob : IAugerJobConfiguration
{
    [AugerJobConfiguration(
        "FraudDetectionJob",
        "Fraud Detection Job Description",
        "Fraud", 
        "v0.0.1")]
    public void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<CardTransaction>("AugerStreamProvider", "input");
        
        var overLimitStream = inputStream.Process<OverLimitDetector>("overLimitStream");
        var expiredCardStream = inputStream.Process<ExpiredCardDetector>("expiredCardStream");
        var normalDistributionStream = inputStream.Process<NormalDistributionDetector>("normalDistributionStream");
        var smallThenLargeStream = inputStream.Process<SmallThenLargeDetector>("smallThenLargeStream");
    }
}