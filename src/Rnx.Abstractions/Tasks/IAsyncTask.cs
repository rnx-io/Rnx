using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Interface for all tasks that are executed in an asynchronous way
    /// </summary>
    public interface IAsyncTask : ITask
    {
        /// <summary>
        /// Notifies that the aynchronous task was completed
        /// </summary>
        event EventHandler<AsyncTaskCompletedEventArgs> Completed;

        /// <summary>
        /// The (unique) id of the asynchronous task
        /// </summary>
        string ExecutionId { get; }
    }
}