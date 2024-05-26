using Orleans.Streams;

namespace WordCount;

public class WordCounter : IAsyncObserver<string>
{
    private int _wordCount = 0;
    private readonly IAsyncObserver<int> _output;
    private readonly ILogger<WordCounter> _logger;

    public WordCounter(IAsyncObserver<int> output, ILogger<WordCounter> logger)
    {
        _output = output;
        _logger = logger;
    }

    public async Task OnNextAsync(string item, StreamSequenceToken? token = null)
    {
        int length = item.Split(' ').Length;
        _wordCount += length;
        await _output.OnNextAsync(_wordCount);
        _logger.LogInformation("Word count for text \"{Text}\": {Length}, Total WordCount: {WordCount}", item, length, _wordCount);
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }
}