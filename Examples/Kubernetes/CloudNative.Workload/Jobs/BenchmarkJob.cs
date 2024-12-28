using CloudNative.Workload.Augers;
using GrainAuger.Abstractions;

namespace CloudNative.Workload.Jobs;

public class BenchmarkJob
{
    [AugerJobConfiguration("BenchmarkJob")]
    static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<CardTransaction, string>("Memory", "inputStream");

        var cpuBoundStream = inputStream.Process<PalindromeAuger>("Kafka");
        var ioBoundStream = inputStream.Process<RemoteAuger>("Kafka");
        var baseStream = inputStream.Process<PassthroughAuger>("Kafka");
    }
}