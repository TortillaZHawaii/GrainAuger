using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
