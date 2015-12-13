using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using Rnx.Core.Tasks;
using System;

namespace Rnx.Core.Execution.Decorators
{
    /// <summary>
    /// Remembers the timestamp when a user-defined task was last run.
    /// </summary>
    public class LastRunTaskDecorator : AbstractTaskDecorator
    {
        private ITaskRunTracker _taskRunTracker;

        public LastRunTaskDecorator(ITaskRunTracker taskRunTracker)
        {
            _taskRunTracker = taskRunTracker;
        }

        public override void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            decoratorQueue.GetNext()?.Execute(decoratorQueue, task, input, output, executionContext);

            if (task is UserDefinedTask)
            {
                _taskRunTracker.LastRunsOfUserDefinedTasks[task.Name] = DateTime.UtcNow;
            }
        }
    }
}