using GrainAuger.Abstractions;

namespace GrainAuger.Core
{
    public class AugerContext : IAugerContext
    {
        private Func<Func<object, Task>, object, TimeSpan, TimeSpan, IDisposable> _registerTimerHandle;

        public AugerContext(Func<Func<object, Task>, object, TimeSpan, TimeSpan, IDisposable> registerTimerHandle)
        {
            _registerTimerHandle = registerTimerHandle;
        }

        public IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state,
            TimeSpan dueTime, TimeSpan period) => _registerTimerHandle(asyncCallback, state, dueTime, period);
    }
}
