using System.Runtime.InteropServices.ComTypes;
using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainAuger.Examples.FraudDetection.WebApi.Producer;

public interface IProducerGrain : IGrainWithStringKey
{
    Task StartAsync();

    Task StopAsync();
}

public class ProducerGrain : Grain, IProducerGrain
{
    private readonly ILogger _logger;
    private IDisposable? _timer;

    public ProducerGrain(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger($"{this.GetType().Name}--{this.IdentityString}");
    }

    public Task StartAsync()
    {
        _logger.LogInformation("StartAsync");
        _timer = this.RegisterTimer(OnTimer, null!, TimeSpan.Zero, TimeSpan.FromMicroseconds(100));
        return Task.CompletedTask;
    }

    private async Task OnTimer(object? state)
    {
        var card = new Card("1234", "Debit", 12, 26, "123");
        var owner = new CardOwner(1, "John", "Smith");
        var transaction = new CardTransaction(12, 100, card, owner);
        
        _logger.LogInformation("OnTimer: {Transaction}", transaction);

        // Route by card number
        var outputStream = this.GetStreamProvider("Kafka")
            .GetStream<CardTransaction>(StreamId.Create("GrainAuger_KafkaInput", card.Number));
        
        await outputStream.OnNextAsync(transaction);
    }

    public Task StopAsync()
    {
        _logger.LogInformation("StopAsync");
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }
}