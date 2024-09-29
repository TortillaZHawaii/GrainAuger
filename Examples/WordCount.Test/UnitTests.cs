using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;
using Orleans.Streams;

namespace WordCount.Test;

public class UnitTests
{
    [TestCase("Hello World", 2)]
    [TestCase("Hello World, this is a test", 6)]
    [TestCase("", 0)]
    public async Task TestWordCount(string item, int expectedWordCount)
    {
        // Arrange
        var output = new Mock<IAsyncObserver<int>>();
        var logger = new Mock<ILogger<WordCounter>>();
        var wordCounter = new WordCounter(output.Object, logger.Object);
        StreamSequenceToken? token = null;
        
        // Act
        await wordCounter.OnNextAsync(item, token);
        
        // Assert
        output.Verify(o => o.OnNextAsync(expectedWordCount, token), Times.Once);
    }
    
    [TestCase(new[] { "Hello", "World" }, new[] { 1, 2 })]
    [TestCase(new[] { "Hello", "World", "this", "is", "a", "test" }, new[] { 1, 2, 3, 4, 5, 6 })]
    [TestCase(new string[0], new int[0])]
    [TestCase(new[] {"Five words in this sentence", "Then extra three"}, new[] {5, 8})]
    public async Task TestConsequentWordCount(string[] items, int[] expectedCounts)
    {
        // Arrange
        var output = new Mock<IAsyncObserver<int>>();
        var logger = new Mock<ILogger<WordCounter>>();
        var wordCounter = new WordCounter(output.Object, logger.Object);
        StreamSequenceToken? token = null;
        
        foreach (var (item, expectedCount) in items.Zip(expectedCounts))
        {
            // Act
            await wordCounter.OnNextAsync(item, token);
            // Assert
            output.Verify(o => o.OnNextAsync(expectedCount, token), Times.Once);
        }
    }
}