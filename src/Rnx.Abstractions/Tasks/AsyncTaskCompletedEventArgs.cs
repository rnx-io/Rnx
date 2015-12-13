using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Eventargs for the Completed event of asynchronous tasks
    /// </summary>
    public class AsyncTaskCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Whether the async task was successful or not
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Error message if task was not successful
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// The executed async task
        /// </summary>
        public IAsyncTask AsyncTask { get; private set; }

        public IExecutionContext ExecutionContext { get; private set; }

        /// <summary>
        /// The output buffer created by the async task
        /// </summary>
        public IBuffer OutputBuffer { get; private set; }

        public AsyncTaskCompletedEventArgs(IAsyncTask asyncTask, IExecutionContext executionContext, IBuffer outputBuffer)
            : this(asyncTask, executionContext, outputBuffer, true, null)
        {}

        public AsyncTaskCompletedEventArgs(IAsyncTask asyncTask, IExecutionContext executionContext, string errorMessage)
            : this(asyncTask, executionContext, null, false, errorMessage)
        {}

        private AsyncTaskCompletedEventArgs(IAsyncTask asyncTask, IExecutionContext executionContext, IBuffer outputBuffer, bool success, string errorMessage)
        {
            AsyncTask = asyncTask;
            ExecutionContext = executionContext;
            Success = success;
            ErrorMessage = errorMessage;
            OutputBuffer = outputBuffer;
        }
    }
}
