namespace ComputeHeavy;

public class HelloGrain : Grain, IHelloGrain
{
    public Task<string> SayHello(string greeting)
    {
        return Task.FromResult($"You said: '{greeting}', I say: Hello!");
    }
}

public interface IHelloGrain : IGrainWithIntegerKey
{
    public Task<string> SayHello(string greeting);
}