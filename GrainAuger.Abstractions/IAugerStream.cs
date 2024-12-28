using System;
using System.Collections.Generic;

namespace GrainAuger.Abstractions
{
    public interface IAugerStream
    {
        IAugerStream Process<T1>(string provider="");
        IAugerStream Process<T1, T2>(string provider = "");
        IAugerStream Process<T1, T2, T3>(string provider = "");
        IAugerStream Process<T1, T2, T3, T4>(string provider = "");
        IAugerStream Process<T1, T2, T3, T4, T5>(string provider = "");
        IAugerStream Process<T1, T2, T3, T4, T5, T6>(string provider = "");
        IAugerStream Process<T1, T2, T3, T4, T5, T6, T7>(string provider = "");
        IAugerStream Process<T1, T2, T3, T4, T5, T6, T7, T8>(string provider = "");
    }
}
