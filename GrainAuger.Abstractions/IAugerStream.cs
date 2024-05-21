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
        
        IAugerStream Process<T1>(string name);
        IAugerStream Process<T1, T2>(string name);
        IAugerStream Process<T1, T2, T3>(string name);
        IAugerStream Process<T1, T2, T3, T4>(string name);
        
        IAugerStream ProcessWithTumblingWindow<T1>(string name, TimeSpan windowSize);
        IAugerStream ProcessWithTumblingWindow<T1, T2>(string name, TimeSpan windowSize);
        IAugerStream ProcessWithTumblingWindow<T1, T2, T3>(string name, TimeSpan windowSize);
        IAugerStream ProcessWithTumblingWindow<T1, T2, T3, T4>(string name, TimeSpan windowSize);
        
        IAugerStream ProcessWithSlidingWindow<T1>(string name, TimeSpan windowSize, TimeSpan slideSize);
        IAugerStream ProcessWithSlidingWindow<T1, T2>(string name, TimeSpan windowSize, TimeSpan slideSize);
        IAugerStream ProcessWithSlidingWindow<T1, T2, T3>(string name, TimeSpan windowSize, TimeSpan slideSize);
        IAugerStream ProcessWithSlidingWindow<T1, T2, T3, T4>(string name, TimeSpan windowSize, TimeSpan slideSize);
        
        IAugerStream ProcessWithSessionWindow<T1>(string name, TimeSpan sessionTimeout);
        IAugerStream ProcessWithSessionWindow<T1, T2>(string name, TimeSpan sessionTimeout);
        IAugerStream ProcessWithSessionWindow<T1, T2, T3>(string name, TimeSpan sessionTimeout);
        IAugerStream ProcessWithSessionWindow<T1, T2, T3, T4>(string name, TimeSpan sessionTimeout);
    }
}
