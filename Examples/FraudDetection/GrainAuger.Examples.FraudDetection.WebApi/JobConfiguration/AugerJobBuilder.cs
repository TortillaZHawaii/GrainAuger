using GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class AugerJobBuilder
{
    public AugerStream<T> FromStream<T>(string providerName, string streamName) where T : class
    {
        return new AugerStream<T>();
    }
    
    public AugerStream<T> ToStream<T>(string providerName, string streamName) where T : class
    {
        return new AugerStream<T>();
    }
}

public class AugerStream<T> where T : class
{
    public AugerStream<TOut> Process<T1, TOut>() where T1 : Auger<T, TOut> where TOut : class
    {
        return new AugerStream<TOut>();
    }
}