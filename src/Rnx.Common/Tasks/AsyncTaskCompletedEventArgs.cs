using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Tasks
{
    public class AsyncTaskCompletedEventArgs : EventArgs
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public IAsyncTask AsyncTask { get; private set; }
        public IExecutionContext ExecutionContext { get; private set; }
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
