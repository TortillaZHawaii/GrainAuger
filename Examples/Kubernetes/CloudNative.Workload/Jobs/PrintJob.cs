// using GrainAuger.Abstractions;
// using Orleans.Streams;
//
// namespace CloudNative.Workload.Jobs;
//
// // Just to test connection to different components
// public class PrintJob
// {
//     [AugerJobConfiguration("PrintJob")]
//     static void Configure(IAugerJobBuilder builder)
//     {
//         var inputStream = builder.FromStream<string, string>("Kafka", "inputStream");
//
//         var printStream = inputStream.Process<PrintProcessor>();
//     }
// }
//
// public class PrintProcessor(IAsyncObserver<string> output, ILogger<PrintProcessor> logger) : IAsyncObserver<string>
// {
//     public async Task OnNextAsync(string item, StreamSequenceToken? token = null)
//     {
//         logger.LogInformation("Processing {Item}", item);
//         try {
//             await output.OnNextAsync(item);
//         } catch (Exception ex) {
//             logger.LogError("Error processing item: {Error}", ex.Message);
//             return;
//         }
//         logger.LogInformation("Processed {Item}", item);
//     }
//
//     public Task OnCompletedAsync()
//     {
//         logger.LogError("Stream completed");
//         return Task.CompletedTask;
//     }
//
//     public Task OnErrorAsync(Exception ex)
//     {
//         logger.LogError("Error processing item: {Error}", ex.Message);
//         return Task.CompletedTask;
//     }
// }
