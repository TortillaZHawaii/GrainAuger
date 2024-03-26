using GrainAuger.Core;
using Orleans.Streams;

namespace GrainAuger.Abstractions.Tests;

public class JobBuilderTests
{
    [Fact]
    public void TestFromStream()
    {
        var builder = new AugerJobBuilder();
        
        var stream = builder.FromStream<object>("provider", "stream");
        
        Assert.NotNull(stream);
        Assert.Equal("stream", stream.Name);
        Assert.Equal("provider", stream.ProviderName);
        
        Assert.Equal(typeof(object), stream.OutputType);
    }

    [Fact]
    public void TestFromStreamInt()
    {
        var builder = new AugerJobBuilder();
        
        var stream = builder.FromStream<int>("provider", "stream");
        
        Assert.NotNull(stream);
        Assert.Equal("stream", stream.Name);
        Assert.Equal("provider", stream.ProviderName);
        
        Assert.Equal(typeof(int), stream.OutputType);
    }
    
    [Fact]
    public void TestFromStreamString()
    {
        var builder = new AugerJobBuilder();
        
        var stream = builder.FromStream<string>("provider", "stream");
        
        Assert.NotNull(stream);
        Assert.Equal("stream", stream.Name);
        Assert.Equal("provider", stream.ProviderName);
        
        Assert.Equal(typeof(string), stream.OutputType);
    }
    
    class InputType
    {
    }
    
    class OutputType
    {
    }

    class Processor : IAsyncObserver<InputType>
    {
        public Processor(IAsyncObserver<OutputType> next)
        {
            
        }
        
        public Task OnNextAsync(InputType item, StreamSequenceToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task OnCompletedAsync()
        {
            throw new NotImplementedException();
        }

        public Task OnErrorAsync(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
    
    [Fact]
    public void TestProcess()
    {
        var builder = new AugerJobBuilder();
        
        var stream = builder
            .FromStream<InputType>("provider", "stream")
            .Process<Processor>("processor");
        
        Assert.NotNull(stream);
        Assert.Equal("processor", stream.Name);
        Assert.Equal("provider", stream.ProviderName);
        
        Assert.Equal(typeof(OutputType), stream.OutputType);
        Assert.Single(stream.Processors);
        Assert.Equal(typeof(Processor), stream.Processors[0]);
    }
}