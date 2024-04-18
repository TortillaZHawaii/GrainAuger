using System;
using System.Threading.Tasks;

namespace GrainAuger.Abstractions
{
    public interface IAugerContext
    {
        IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state,
            TimeSpan dueTime, TimeSpan period);
    }
}
