// this will be <auto-generated>

using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateAfter;

[ImplicitStreamSubscription("GrainAuger_KafkaInput")] // stream namespace input should be the same as the output of the previous grain
public class GrainAugerExpiredCardDetector 
    : Grain, // to make use of the Orleans runtime
    IGrainWithGuidKey, // key type depends on keyed stream type, for example when we run KeyedStream based on CardId
// we should use string, this is required for auto activation
// EXAMPLE:
// we produce to stream namespace: GrainAuger_vXXX_EntryPoint_Output, to streamId: CardId
// this would wake up ALL listeners of different types, but with the same key
    IAsyncObserver<CardTransaction>
{
    private readonly ExpiredCardDetector _expiredCardDetector; // this is the actual detector, this should be generated
    
    private IAsyncStream<Alert> _outputStream; // this is the output stream, this should be generated
    
    private readonly ILogger<GrainAugerExpiredCardDetector> _logger;
    
    public GrainAugerExpiredCardDetector(
        // We could inject here some dependencies for the detector
        ILogger<GrainAugerExpiredCardDetector> logger
        )
    {
        _expiredCardDetector = new ExpiredCardDetector();
        _logger = logger;
    }
    
    // the rest is pretty much boilerplate

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating...");
        
        await base.OnActivateAsync(cancellationToken);

        var inputStreamProvider = this.GetStreamProvider("Kafka");
        var inputStreamId = StreamId.Create("GrainAuger_KafkaInput", this.GetPrimaryKey());
        var inputStream = inputStreamProvider.GetStream<CardTransaction>(inputStreamId);
        
        var outputStreamProvider = this.GetStreamProvider("Kafka");
        var outputStreamId = StreamId.Create("GrainAuger_ExpiredCardDetector_Output", this.GetPrimaryKey());
        _outputStream = outputStreamProvider.GetStream<Alert>(outputStreamId);        

        await inputStream.SubscribeAsync(this);
        
        _logger.LogInformation("Activated");
    }

    public async Task OnNextAsync(CardTransaction item, StreamSequenceToken token = null)
    {
        _logger.LogInformation("Processing {Transaction}", item);
        // chain the detectors
        await _expiredCardDetector.ProcessAsync(item, 
            async alert =>
            {
                await _outputStream.OnNextAsync(alert);
                _logger.LogInformation("Alert: {Alert}", alert);
            });
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public async Task OnErrorAsync(Exception ex)
    {
        // push the exception to the output stream
        await _outputStream.OnErrorAsync(ex);
    }
}
