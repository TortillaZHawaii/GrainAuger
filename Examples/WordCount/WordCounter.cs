using Orleans.Streams;

namespace WordCount;

class WordCounter(IAsyncObserver<int> output, ILogger<WordCounter> logger
    ) : IAsyncObserver<string>
{
    private int _wordCount = 0;
    
    public async Task OnNextAsync(string item, StreamSequenceToken? token = null)
    {
        int length = item.Split(' ').Length;
        _wordCount += length;
        await output.OnNextAsync(_wordCount);
        logger.LogInformation("Word count for text \"{Text}\": {Length}, Total WordCount: {WordCount}", item, length, _wordCount);
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