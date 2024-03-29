﻿using System;
using System.Threading.Tasks;

namespace GrainAuger.Abstractions
{
    public interface IGrainContext
    {
        IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state,
            TimeSpan dueTime, TimeSpan period);
    }
}
