using GrainAuger.Abstractions;
using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class FraudDetectionJob
{
    [AugerJobConfiguration("FraudDetectionJob")]
    public static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<CardTransaction, string>("Kafka", "GrainAuger_KafkaInput");

        IAugerStream overLimitStream = inputStream.Process<OverLimitDetector>();
        var expiredCardStream = inputStream.Process<ExpiredCardDetector>();
        var normalDistributionStream = inputStream.Process<NormalDistributionDetector>();
        var smallThenLargeStream = inputStream.Process<SmallThenLargeDetector>();
        
        var chainedSum = inputStream.Process<MoneyFromTransactionGetter, MoneyFromTransactionsAggregator>();
    }
}