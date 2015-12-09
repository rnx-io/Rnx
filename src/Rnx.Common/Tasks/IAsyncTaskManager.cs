using Rnx.Common.Execution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rnx.Common.Tasks
{
    public interface IAsyncTaskManager
    {
        bool HasUncompletedTasks { get; }
        void RegisterAsyncExecution(IAsyncTask asyncTask, IExecutionContext executionContext);
        void WaitForTaskCompletion(IExecutionContext executionContext);
        AsyncTaskCompletedEventArgs WaitForTaskCompletion(IExecutionContext executionContext, string executionId);
        void WaitAll();
    }
}