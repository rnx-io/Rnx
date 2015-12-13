using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using Rnx.Core.Execution.Decorators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rnx.Core.Execution
{
    /// <summary>
    /// Default implementation for <see cref="ITaskExecuter"/>. Also implements the <see cref="ITaskDecorator"/>-interface
    /// so that it can be part of the task execution decorator workflow
    /// </summary>
    public class DefaultTaskExecuter : ITaskExecuter, ITaskDecorator
    {
        public Type ExportType => GetType();

        private IEnumerable<ITaskDecorator> _taskDecorators;

        public DefaultTaskExecuter(IEnumerable<ITaskDecorator> taskDecorators)
        {
            _taskDecorators = taskDecorators;
        }

        public void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            // find all task-specific decorators that were registered for this type of task
            var taskSpecificDecorators = task is ITaskDecorationProvider ? ((ITaskDecorationProvider)task).TaskDecorators : Enumerable.Empty<ITaskDecorator>();

            // select all common decorators, except those that are overwritten by task-specific decorators
            // merge these with all task-specific decorators and finally with the task executer ("this")
            var decoratorQueue = new DecoratorQueue(_taskDecorators.Where(f => !taskSpecificDecorators.Select(x => x.ExportType).Contains(f.ExportType))
                                                                   .Concat(taskSpecificDecorators)
                                                                   .Concat(Enumerable.Repeat(this, 1)));

            // get the first decorator of the queue (will be "this" when no decorators are present)
            // and call execute on that decorator
            decoratorQueue.GetNext().Execute(decoratorQueue, task, input, output, executionContext);
        }

        void ITaskDecorator.Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            // this is the final point where a task will be actually executed
            task.Execute(input, output, executionContext);
        }
    }
}