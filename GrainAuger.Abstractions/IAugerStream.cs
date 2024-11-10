using System;
using System.Collections.Generic;

namespace GrainAuger.Abstractions
{
    public interface IAugerStream
    {
        string Name { get; }
        string ProviderName { get; }
        
        Type OutputType { get; }
        
        IReadOnlyList<Type> Processors { get; }
        
        IAugerStream Process<T1>();
        IAugerStream Process<T1, T2>();
        IAugerStream Process<T1, T2, T3>();
        IAugerStream Process<T1, T2, T3, T4>();
        
        // Prefixes previous stream with TumblingWindow mechanism
        IAugerStream WithTumblingWindow(TimeSpan windowSize);
        
        // Prefixes previous stream with SlidingWindow mechanism
        IAugerStream WithSlidingWindow(TimeSpan windowSize, TimeSpan slideSize);
        
        // Prefixes previous stream with SessionWindow mechanism
        IAugerStream WithSessionWindow(TimeSpan sessionTimeout);
        
        // Postfixes previous stream with Round Robin load balancing mechanism
        IAugerStream WithRoundRobinLoadBalancer(int bucketCount);
        
        // Postfixes previous stream with Random load balancing mechanism
        IAugerStream WithRandomLoadBalancer(int bucketCount);
        
        // Postfixes previous stream with KeyBy balancing mechanism
        IAugerStream KeyBy<T, TKey>(Func<T, TKey> keySelector);
    }
}
