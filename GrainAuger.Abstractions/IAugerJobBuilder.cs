using System;

namespace GrainAuger.Abstractions
{
    public interface IAugerJobBuilder
    {
        // TKey is either string, Guid or long
        IAugerStream FromStream<TData, TKey>(string providerName, string streamName) where TData : notnull where TKey : notnull;
    }
}