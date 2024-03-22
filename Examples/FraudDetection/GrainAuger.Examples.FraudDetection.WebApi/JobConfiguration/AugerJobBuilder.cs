using GrainAuger.Examples.FraudDetection.WebApi.GenerateBefore;
using Orleans.Runtime;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class AugerJobBuilder
{
    public AugerStream<T> FromStream<T>(string providerName, string streamName) where T : class
    {
        throw new NotImplementedException();
    }
    
    public AugerStream<T> ToStream<T>(string providerName, string streamName) where T : class
    {
        throw new NotImplementedException();
    }
}

public class AugerStream<T> where T : class
{
    public AugerStream<TOut> Process<T1, TOut>() where T1 : Auger<T, TOut> where TOut : class
    {
        throw new NotImplementedException();
    }
}