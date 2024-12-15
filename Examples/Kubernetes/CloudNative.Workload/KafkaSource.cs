using System.Text.Json;
using Confluent.Kafka;

namespace CloudNative.Workload;

public class KafkaSource : IHostedService
{
    private readonly ILogger<KafkaSource> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly string _kafkaBroker;
    private readonly string _topic;
    private IConsumer<string, string> _consumer;

    public KafkaSource(ILogger<KafkaSource> logger, IClusterClient clusterClient, string kafkaBroker, string topic)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _kafkaBroker = kafkaBroker;
        _topic = topic;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = "auger",
            BootstrapServers = _kafkaBroker,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true,
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(_topic);

        Task.Run(() => ConsumeKafkaMessages(cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    private async Task ConsumeKafkaMessages(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                _logger.LogInformation("Consumed message with key {Key} and value {Value}", consumeResult.Message.Key, consumeResult.Message.Value);
                var streamProvider = _clusterClient.GetStreamProvider("Kafka");
                var stream = streamProvider.GetStream<CardTransaction>(StreamId.Create("inputStream", consumeResult.Message.Key));
                var dto = JsonSerializer.Deserialize<CardTransaction>(consumeResult.Message.Value);
                if (dto == null)
                {
                    _logger.LogWarning("Failed to deserialize message {Value}", consumeResult.Message.Value);
                    continue;
                }
                _logger.LogInformation("Sending transaction {Id}: {CardNumber} {Amount}", dto.Id, dto.CardNumber, dto.Amount);
                await stream.OnNextAsync(dto);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
                _logger.LogInformation("Kafka consumer cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming Kafka message");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        _consumer.Dispose();
        return Task.CompletedTask;
    }
}