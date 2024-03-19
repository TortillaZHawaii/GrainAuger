using GrainAuger.Examples.FraudDetection.WebApi.Dtos;
using Orleans.Runtime;

namespace GrainAuger.Examples.FraudDetection.WebApi.Utils;

public interface IProducerGrain : IGrainWithStringKey
{
    Task StartAsync();

    Task StopAsync();
}

public class ProducerGrain : Grain, IProducerGrain
{
    private readonly ILogger _logger;
    private IDisposable? _timer;
    private readonly Random _random = new();

    public ProducerGrain(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger($"{this.GetType().Name}--{this.IdentityString}");
    }

    public Task StartAsync()
    {
        _logger.LogInformation("StartAsync");
        _timer = this.RegisterTimer(OnTimer, null!, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        return Task.CompletedTask;
    }

    private async Task OnTimer(object? state)
    {
        int amount = _random.Next(1, 1000);
        var card = new Card("1234", "Debit", 12, 2020, "123");
        var owner = new CardOwner(1, "John", "Smith");
        var transaction = new CardTransaction(amount, 100, card, owner);
        
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