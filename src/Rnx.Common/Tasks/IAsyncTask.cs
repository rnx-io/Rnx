using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Tasks
{
    public interface IAsyncTask : ITask
    {
        event EventHandler<AsyncTaskCompletedEventArgs> Completed;
        string ExecutionId { get; }
    }
}
