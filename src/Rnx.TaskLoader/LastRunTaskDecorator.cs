using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using System;

namespace Rnx.TaskLoader
{
    /// <summary>
    /// Remembers the timestamp when a user-defined task was last run.
    /// </summary>
    public class LastRunTaskDecorator : AbstractTaskDecorator
    {
        private readonly ITaskRunTracker _taskRunTracker;

        public LastRunTaskDecorator(ITaskRunTracker taskRunTracker)
        {
            _taskRunTracker = taskRunTracker;
        }

        public override void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            decoratorQueue.GetNext()?.Execute(decoratorQueue, task, input, output, executionContext);

            var userDefinedTask = task as UserDefinedTask;

            if (userDefinedTask != null)
            {
                _taskRunTracker.LastTaskRuns[userDefinedTask.TaskDescriptor] = DateTime.UtcNow;
            }
        }
    }
}