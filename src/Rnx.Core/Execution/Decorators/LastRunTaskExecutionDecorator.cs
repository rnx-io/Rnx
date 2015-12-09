using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Execution.Decorators;
using Rnx.Common.Tasks;
using Rnx.Core.Tasks;
using System;

namespace Rnx.Core.Execution.Decorators
{
    public class LastRunTaskExecutionDecorator : AbstractTaskExecutionDecorator
    {
        private ITaskRunTracker _taskRunTracker;

        public LastRunTaskExecutionDecorator(ITaskRunTracker taskRunTracker)
        {
            _taskRunTracker = taskRunTracker;
        }

        public override void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            Decoratee.Execute(task, input, output, executionContext);

            if (task is UserDefinedTask)
            {
                _taskRunTracker.LastRuns[task.Name] = DateTime.UtcNow;
            }
        }
    }
}