using System;
using System.Collections.Generic;
using System.Text;

namespace GrainAuger.Abstractions
{
    public interface IAugerStream
    {
        IAugerStream Process<T1>();


        IAugerStream Process<T1, T2>();


        IAugerStream Process<T1, T2, T3>();


        IAugerStream Process<T1, T2, T3, T4>();
    }
}
