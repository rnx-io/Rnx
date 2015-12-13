using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Threading
{
    public class AwaitTask : RnxTask
    {
        private string _executionId;
        private Action<AsyncTaskCompletedEventArgs, IBuffer, IBuffer, IExecutionContext> _action;

        public AwaitTask()
        { }

        public AwaitTask(string executionId, Action<AsyncTaskCompletedEventArgs, IBuffer, IBuffer, IExecutionContext> action = null)
        {
            _executionId = executionId;
            _action = action;
        }
        
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var asyncManager = GetAsyncTaskManager(executionContext);
            var eventArgs = asyncManager.WaitForTaskCompletion(executionContext.UserDefinedTaskName, _executionId);

            _action?.Invoke(eventArgs, input, output, executionContext);
        }

        protected virtual IAsyncTaskManager GetAsyncTaskManager(IExecutionContext ctx) => RequireService<IAsyncTaskManager>(ctx);
    }
}