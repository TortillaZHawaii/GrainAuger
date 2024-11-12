using System;
using System.Collections.Generic;

namespace GrainAuger.Abstractions
{
    public interface IAugerStream
    {
        IAugerStream Process<T1>();
        IAugerStream Process<T1, T2>();
        IAugerStream Process<T1, T2, T3>();
        IAugerStream Process<T1, T2, T3, T4>();
        IAugerStream Process<T1, T2, T3, T4, T5>();
        IAugerStream Process<T1, T2, T3, T4, T5, T6>();
        IAugerStream Process<T1, T2, T3, T4, T5, T6, T7>();
        IAugerStream Process<T1, T2, T3, T4, T5, T6, T7, T8>();
    }
}
