using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Execution.Decorators;
using Rnx.Common.Tasks;
using System.Reflection;
using System;

namespace Rnx.Core.Execution.Decorators
{
    public class AsyncTaskExecutionDecorator : AbstractTaskExecutionDecorator
    {
        private IAsyncTaskManager _asyncTaskManager;

        public AsyncTaskExecutionDecorator(IAsyncTaskManager asyncTaskManager)
        {
            _asyncTaskManager = asyncTaskManager;
        }
        
        public override void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            if (typeof(IAsyncTask).IsAssignableFrom(task.GetType()))
            {
                var asyncTask = (IAsyncTask)task;

                _asyncTaskManager.RegisterAsyncExecution(asyncTask, executionContext);
            }

            Decoratee.Execute(task, input, output, executionContext);
        }
    }
}