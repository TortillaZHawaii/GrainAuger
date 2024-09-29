using GrainAuger.Abstractions;

namespace WordCount;

abstract class WordCountJobConfigurationWithStopper
{
    [AugerJobConfiguration("WordCountJob")]
    static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<string, string>("MemoryStream", "WordCountInput");
        
        // This auger will only pass through items if the "WordFilterJob" feature flag is enabled.
        var enabledStream = inputStream
            .Process<JobStopperAuger>("jobStopperStream");
        
        // Rest of the job configuration
        var wordCountStream = enabledStream
            .Process<WordCounter>("wordCounterStream");
    }
}
