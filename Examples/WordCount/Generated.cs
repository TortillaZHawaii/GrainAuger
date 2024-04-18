// namespace WordCount;
//
// [global::Orleans.ImplicitStreamSubscription("WordCountInput")]
// internal class wordCountStream2 :
//     global::Orleans.Grain,
//     global::Orleans.IGrainWithStringKey,
//     global::Orleans.Streams.IAsyncObserver<global::System.String>
// {
//     private readonly global::Microsoft.Extensions.Logging.ILogger<wordCountStream2> _logger;
//     private global::Orleans.Streams.IAsyncStream<global::System.Int32> _outputStream;
//     private readonly global::WordCount.WordCounter _processor0;
//     
//     internal wordCountStream2(
//         global::Microsoft.Extensions.Logging.ILogger<wordCountStream2> logger,
// 		global::Microsoft.Extensions.Logging.ILogger<global::WordCount.WordCounter> v1
//         )
//     {
//         _logger = logger;
//         _processor0 = new global::WordCount.WordCounter(_outputStream, v1);
//     }
//     
//     public override async Task OnActivateAsync(CancellationToken cancellationToken)
//     {
//         _logger.LogInformation("Activating...");
//         
//         await base.OnActivateAsync(cancellationToken);
//         
//         var inputStreamProvider = this.GetStreamProvider("MemoryStream");
//         var inputStreamId = global::Orleans.Runtime.StreamId.Create("WordCountInput", this.GetPrimaryKeyString());
//         var inputStream = inputStreamProvider.GetStream<global::System.String>(inputStreamId);
//         
//         var outputStreamProvider = this.GetStreamProvider("MemoryStream");
//         var outputStreamId = global::Orleans.Runtime.StreamId.Create("wordCounterStream", this.GetPrimaryKeyString());
//         _outputStream = outputStreamProvider.GetStream<global::System.Int32>(outputStreamId);
//         
//         await inputStream.SubscribeAsync(this);
//         
//         _logger.LogInformation("Activated");
//     }
//     
//     public async Task OnNextAsync(global::System.String item, global::Orleans.Streams.StreamSequenceToken token = null)
//     {
//         _logger.LogInformation("Processing {item}", item);
//         // chain the processors
//         
//     }
//     
//     public Task OnCompletedAsync()
//     {
//         return Task.CompletedTask;
//     }
//     
//     public async Task OnErrorAsync(Exception ex)
//     {
//         // push the exception to the output stream
//         await _outputStream.OnErrorAsync(ex);
//     }
// }