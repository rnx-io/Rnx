using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using System.Reflection;
using System;

namespace Rnx.Core.Execution.Decorators
{
    /// <summary>
    /// Decorator to intercept tasks and check whether they implement the <see cref="IAsyncTask"/> interface.
    /// If so, the task will be registered via the <see cref="IAsyncTaskManager"/>.
    /// </summary>
    public class AsyncTaskDecorator : AbstractTaskDecorator
    {
        private IAsyncTaskManager _asyncTaskManager;

        public AsyncTaskDecorator(IAsyncTaskManager asyncTaskManager)
        {
            _asyncTaskManager = asyncTaskManager;
        }
        
        public override void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            if (typeof(IAsyncTask).IsAssignableFrom(task.GetType()))
            {
                _asyncTaskManager.RegisterAsyncExecution((IAsyncTask)task, executionContext.UserDefinedTaskName);
            }

            decoratorQueue.GetNext()?.Execute(decoratorQueue, task, input, output, executionContext);
        }
    }
}