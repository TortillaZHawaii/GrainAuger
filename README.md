# Grain Auger

Declarative dataflow language over Orleans streams.

## Overview

Grain Auger is a declarative dataflow language over Orleans streams. It allows you to define a dataflow graph using a simple DSL and then execute it on an Orleans cluster. The dataflow graph is defined as a directed acyclic graph (DAG) where each node is a grain and each edge is a virtual stream. The language is designed to be simple and expressive, allowing you to define complex dataflows with minimal effort and boilerplate.

## Example

Here is an example of a simple word count dataflow defined using Grain Auger:

```csharp
abstract class WordCountJobConfiguration
{
    [AugerJobConfiguration("WordCountJob")]
    static void Configure(IAugerJobBuilder builder)
    {
        var inputStream = builder.FromStream<string, string>("MemoryStream", "WordCountInput");

        var wordCountStream = inputStream.Process<WordCounter>("wordCounterStream");
    }
}
```

WordCounter is a simple class that implements `IAsyncObserver<string>` and can be tested in isolation:

```csharp
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
```

Based on the configuration, Grain Auger will generate the necessary Orleans grains and streams to execute the dataflow on an Orleans cluster. The dataflow will read input from a stream named "WordCountInput", process it using the WordCounter grain, and write the output to a stream named "wordCounterStream".

<details>
    <summary>Generated code</summary>
    
```csharp
// <auto-generated/>

namespace GrainAugerCodeGen.WordCount;

/*
Found Dag for job WordCountJob:
Foreign Source <string> -> inputStream
inputStream -[WordCount.WordCounter]-> wordCountStream
*/

/*
Found constructors:
WordCount.WordCounter.WordCounter(Orleans.Streams.IAsyncObserver<int>, Microsoft.Extensions.Logging.ILogger<WordCount.WordCounter>)
*/

[global::Orleans.ImplicitStreamSubscription("WordCountInput")]
internal class wordCountStream :
global::Orleans.Grain,
global::Orleans.IGrainWithStringKey,
global::Orleans.Streams.IAsyncObserver<global::System.String>
{
private readonly global::Microsoft.Extensions.Logging.ILogger<wordCountStream> _logger;
private global::Orleans.Streams.IAsyncStream<global::System.Int32> _outputStream;
private readonly global::WordCount.WordCounter _processor0;

    internal wordCountStream(
        global::Microsoft.Extensions.Logging.ILogger<wordCountStream> logger,
        global::Microsoft.Extensions.Logging.ILogger<global::WordCount.WordCounter> v1
        )
    {
        _logger = logger;
        _processor0 = new global::WordCount.WordCounter(_outputStream, v1);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating...");

        await base.OnActivateAsync(cancellationToken);

        var inputStreamProvider = this.GetStreamProvider("MemoryStream");
        var inputStreamId = global::Orleans.Runtime.StreamId.Create("WordCountInput", this.GetPrimaryKeyString());
        var inputStream = inputStreamProvider.GetStream<global::System.String>(inputStreamId);

        var outputStreamProvider = this.GetStreamProvider("MemoryStream");
        var outputStreamId = global::Orleans.Runtime.StreamId.Create("wordCounterStream", this.GetPrimaryKeyString());
        _outputStream = outputStreamProvider.GetStream<global::System.Int32>(outputStreamId);

        await inputStream.SubscribeAsync(this);

        _logger.LogInformation("Activated");
    }

    public async Task OnNextAsync(global::System.String item, global::Orleans.Streams.StreamSequenceToken token = null)
    {
        _logger.LogInformation("Processing {item}", item);
        // chain the processors
        await _processor0.OnNextAsync(item, token);
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public async Task OnErrorAsync(Exception ex)
    {
        // push the exception to the output stream
        await _outputStream.OnErrorAsync(ex);
    }

}
```

</details>

## Related Work

- [Microsoft Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/): Orleans is a framework for building distributed systems in .NET. It provides a simple programming model for building distributed applications using the actor model. Orleans streams are a powerful abstraction for building scalable and fault-tolerant data processing pipelines.
- [Apache Beam](https://beam.apache.org/): Beam is a unified programming model for building batch and streaming data processing pipelines. It provides a high-level API for defining data processing pipelines that can be executed on a variety of distributed processing backends.
- [Apache Flink](https://flink.apache.org/): Flink is a distributed stream processing framework for building real-time data processing applications. It provides a high-level API for defining data processing pipelines that can be executed on a distributed cluster.
```
