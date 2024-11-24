using GrainAuger.Abstractions;
using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using GrainAuger.LoadBalancers;
using GrainAuger.Windows;
using Orleans.Streams;

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
        var quickTravelStream = inputStream.Process<SessionWindow, QuickTravelDetector>();
        
        var chainedSum = inputStream.Process<MoneyFromTransactionGetter, MoneyFromTransactionsAggregator>();
    }
}

public class SessionWindow(IAsyncObserver<List<CardTransaction>> output, IAugerContext context) 
    : SessionWindowAuger<CardTransaction>(TimeSpan.FromMinutes(1), output, context);
    
public class TumblingExample(IAsyncObserver<List<CardTransaction>> output, IAugerContext context)
    : TumblingWindowAuger<CardTransaction>(TimeSpan.FromSeconds(10), output, context);