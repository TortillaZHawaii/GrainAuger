using Orleans.Streams;

namespace WordCount;

class WordCounter(IAsyncObserver<int> output) : IAsyncObserver<string>
{
    private int _wordCount = 0;
    
    public async Task OnNextAsync(string item, StreamSequenceToken? token = null)
    {
        _wordCount += item.Split(' ').Length;
        await output.OnNextAsync(_wordCount);
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