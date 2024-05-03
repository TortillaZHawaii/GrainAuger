using System.Collections.Frozen;
using GrainAuger.Abstractions;
using Orleans.Streams;

namespace GrainAuger.ParallelKeysBenchmark;

public class RomanToIntegerTransform(IAsyncObserver<int> output) : IAsyncObserver<string>
{
    static FrozenDictionary<char, int> RomanMap = new Dictionary<char, int>
    {
        {'I', 1},
        {'V', 5},
        {'X', 10},
        {'L', 50},
        {'C', 100},
        {'D', 500},
        {'M', 1000}
    }.ToFrozenDictionary();
    
    private int RomanToInteger(string item)
    {
        int number = 0;
        item = item
            .Replace("IV", "IIII")
            .Replace("IX", "VIIII")
            .Replace("XL", "XXXX")
            .Replace("XC", "LXXXX")
            .Replace("CD", "CCCC")
            .Replace("CM", "DCCCC");
        foreach (var c in item)
            number += RomanMap[c];
        return number;
    }
    
    public Task OnNextAsync(string item, StreamSequenceToken? token = null)
    {
        return Task.FromResult(RomanToInteger(item));
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

public class Job
{
    [AugerJobConfiguration("RomanToIntegerJob")]
    public void Configure(IAugerJobBuilder builder)
    {
        var input = builder.FromStream<string, int>("Provider", "Stream");
        var intsStream = input.Process<RomanToIntegerTransform>("RomanToIntegerTransform");
    }
}
