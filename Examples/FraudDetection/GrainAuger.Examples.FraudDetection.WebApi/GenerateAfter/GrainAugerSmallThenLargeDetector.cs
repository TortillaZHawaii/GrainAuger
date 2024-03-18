using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateAfter;

[ImplicitStreamSubscription("GrainAuger_KafkaInput")]
public class GrainAugerSmallThenLargeDetector : Grain, IGrainWithGuidKey, IAsyncObserver<CardTransaction>
{
    private readonly SmallThenLargeDetector _smallThenLargeDetector;
    
    private IAsyncStream<Alert> _outputStream;

    private readonly ILogger<GrainAugerSmallThenLargeDetector> _logger;
    
    public GrainAugerSmallThenLargeDetector(
        [PersistentState("SmallThenLargeDetector", "AugerStore")]
        IPersistentState<SmallThenLargeDetectorState> state,
        ILogger<GrainAugerSmallThenLargeDetector> logger,
        ILogger<SmallThenLargeDetector> smallThenLargeDetectorLogger
        )
    {
        _smallThenLargeDetector = new SmallThenLargeDetector(state, smallThenLargeDetectorLogger);
        _logger = logger;
    }
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating...");
        
        await base.OnActivateAsync(cancellationToken);

        var inputStreamProvider = this.GetStreamProvider("Kafka");
        var inputStreamId = StreamId.Create("GrainAuger_KafkaInput", this.GetPrimaryKey());
        var inputStream = inputStreamProvider.GetStream<CardTransaction>(inputStreamId);
        
        var outputStreamProvider = this.GetStreamProvider("Kafka");
        var outputStreamId = StreamId.Create("GrainAuger_SmallThenLargeDetector_Output", this.GetPrimaryKey());
        _outputStream = outputStreamProvider.GetStream<Alert>(outputStreamId);

        _smallThenLargeDetector.RegisterTimerHandle = this.RegisterTimer;
        
        await inputStream.SubscribeAsync(this);
        
        _logger.LogInformation("Activated");
    }

    public async Task OnNextAsync(CardTransaction item, StreamSequenceToken? token = null)
    {
        _logger.LogInformation("Processing {Transaction}", item);
        // chain the detectors
        await _smallThenLargeDetector.ProcessAsync(item, 
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
