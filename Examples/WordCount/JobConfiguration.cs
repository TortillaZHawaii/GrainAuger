using GrainAuger.Abstractions;

namespace WordCount;

abstract class WordCountJobConfiguration
{
    [AugerJobConfiguration("WordCountJob")]
    static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<string, string>("MemoryStream", "WordCountInput");

        var wordCountStream = inputStream.Process<WordCounter>("wordCounterStream");
    }
}
