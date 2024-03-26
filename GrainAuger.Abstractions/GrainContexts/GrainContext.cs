using System;
using System.Threading.Tasks;

namespace GrainAuger.Abstractions.GrainContexts
{
    public class GrainContext : IGrainContext
    {
        private Func<Func<object, Task>, object, TimeSpan, TimeSpan, IDisposable> _registerTimerHandle;

        public GrainContext(Func<Func<object, Task>, object, TimeSpan, TimeSpan, IDisposable> registerTimerHandle)
        {
            _registerTimerHandle = registerTimerHandle;
        }

        public IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state,
            TimeSpan dueTime, TimeSpan period) => _registerTimerHandle(asyncCallback, state, dueTime, period);
    }
}
