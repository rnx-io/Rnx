using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Threading
{
    public class AwaitTaskDescriptor : TaskDescriptorBase<AwaitTask>
    {
        internal string ExecutionId { get; }
        internal Action<AsyncTaskCompletedEventArgs, IBuffer, IBuffer, IExecutionContext> Action { get; }

        public AwaitTaskDescriptor()
        { }

        public AwaitTaskDescriptor(string executionId, Action<AsyncTaskCompletedEventArgs, IBuffer, IBuffer, IExecutionContext> action = null)
        {
            if(string.IsNullOrWhiteSpace(executionId))
            {
                throw new ArgumentException($"{nameof(executionId)} must not be null or empty");
            }

            ExecutionId = executionId;
            Action = action;
        }
    }

    public class AwaitTask : RnxTask
    {
        private readonly AwaitTaskDescriptor _awaitTaskDescriptor;
        private readonly IAsyncTaskManager _asyncTaskManager;

        public AwaitTask(AwaitTaskDescriptor awaitTaskDescriptor, IAsyncTaskManager asyncTaskManager)
        {
            _awaitTaskDescriptor = awaitTaskDescriptor;
            _asyncTaskManager = asyncTaskManager;
        }
        
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var eventArgs = _asyncTaskManager.WaitForTaskCompletion(executionContext.RootTaskDescriptor, _awaitTaskDescriptor.ExecutionId);

            _awaitTaskDescriptor.Action?.Invoke(eventArgs, input, output, executionContext);
        }
    }
}