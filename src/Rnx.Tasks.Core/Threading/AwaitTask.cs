using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Common.Buffers;
using Rnx.Common.Execution;

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
            var asyncManager = GetService<IAsyncTaskManager>(executionContext);
            var eventArgs = asyncManager.WaitForTaskCompletion(executionContext, _executionId);

            _action?.Invoke(eventArgs, input, output, executionContext);
        }
    }
}