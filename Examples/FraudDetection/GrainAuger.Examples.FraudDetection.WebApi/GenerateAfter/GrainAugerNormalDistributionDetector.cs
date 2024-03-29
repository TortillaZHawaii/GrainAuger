using GrainAuger.Examples.FraudDetection.WebApi.Detectors;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.GenerateAfter;

[ImplicitStreamSubscription("GrainAuger_KafkaInput")]
public class GrainAugerNormalDistributionDetector
    : Grain, IGrainWithStringKey, IAsyncObserver<CardTransaction>
{
    private readonly NormalDistributionDetector _normalDistributionDetector;
    
    private IAsyncStream<Alert> _outputStream;

    public GrainAugerNormalDistributionDetector(
        // based on the detector dependencies - this is different for each detector
        [PersistentState("NormalDistributionDetector", "AugerStore")]
        IPersistentState<NormalDistributionState> state
        )
    {
        _normalDistributionDetector = new NormalDistributionDetector(state);
    }
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        var inputStreamProvider = this.GetStreamProvider("Kafka");
        var inputStreamId = StreamId.Create("GrainAuger_KafkaInput", this.GetPrimaryKeyString());
        var inputStream = inputStreamProvider.GetStream<CardTransaction>(inputStreamId);

        var outputStreamProvider = this.GetStreamProvider("Kafka");
        var outputStreamId = StreamId.Create("GrainAuger_NormalDistributionDetector_Output", this.GetPrimaryKeyString());
        _outputStream = outputStreamProvider.GetStream<Alert>(outputStreamId);

        await inputStream.SubscribeAsync(this);
    }

    public async Task OnNextAsync(CardTransaction item, StreamSequenceToken? token = null)
    {
        await _normalDistributionDetector.ProcessAsync(item, 
            async alert => await _outputStream.OnNextAsync(alert)
        );
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public async Task OnErrorAsync(Exception ex)
    {
        await _outputStream.OnErrorAsync(ex);
    }
}