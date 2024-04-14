using GrainAuger.Abstractions;

namespace WordCount;

abstract class JobConfiguration
{
    [AugerJobConfiguration("WordCountJob")]
    static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<string>("MemoryStream", "input");

        var wordCountStream = inputStream.Process<WordCounter>("wordCounterStream");
    }
}
