namespace CloudNative.Workload.Grains;

public class HelloGrain : Grain, IHelloGrain
{
    public Task<string> SayHello()
    {
        var hostname = Environment.MachineName;
        var response = $"Hello from {hostname}, you said: '{this.GetPrimaryKeyString()}', I say: Hello!";
        return Task.FromResult(response);
    }
}

public interface IHelloGrain : IGrainWithStringKey
{
    public Task<string> SayHello();
}