using GrainAuger.Abstractions;
using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using GrainAuger.LoadBalancers;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class FraudDetectionJob
{
    [AugerJobConfiguration("FraudDetectionJob")]
    public static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<CardTransaction, string>("Kafka", "GrainAuger_KafkaInput");

        IAugerStream overLimitStream = inputStream
            // .WithSessionWindow(TimeSpan.FromMinutes(1))
            .Process<OverLimitDetector>();
            // .WithRandomLoadBalancer(123);
        var expiredCardStream = inputStream.Process<ExpiredCardDetector>();
        var normalDistributionStream = inputStream.Process<NormalDistributionDetector>();
        var smallThenLargeStream = inputStream.Process<SmallThenLargeDetector>();
        
        var chainedSum = inputStream.Process<MoneyFromTransactionGetter, MoneyFromTransactionsAggregator>();
    }
}