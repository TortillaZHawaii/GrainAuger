using CloudNative.Workload.Augers;
using GrainAuger.Abstractions;

namespace CloudNative.Workload.Jobs;

public class BenchmarkJob
{
    [AugerJobConfiguration("BenchmarkJob")]
     static void Configure(IAugerJobBuilder builder)
     {
         var inputStream = builder.FromStream<CardTransaction, string>("Kafka", "inputStream");

         var cpuBoundStream = inputStream.Process<PalindromeAuger>();
         var ioBoundStream = inputStream.Process<RemoteAuger>();
         var baseStream = inputStream.Process<PassthroughAuger>();
     }
}