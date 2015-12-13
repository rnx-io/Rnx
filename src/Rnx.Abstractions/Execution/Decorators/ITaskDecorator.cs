using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Execution.Decorators
{
    /// <summary>
    /// Enables separation of concerns, by putting cross-cutting concerns into task decorators.
    /// Before any task is executed via an <see cref="ITaskExecuter"/>, all available decorators are registered
    /// and will decorate the execution of a task. An examplary use case is a logging decorator, that puts logging
    /// statements before and after the actually task execution.
    /// </summary>
    public interface ITaskDecorator
    {
        /// <summary>
        /// Used to register the current decorator for a specific type. This is necessary to later be able to overwrite
        /// decorators with task-specific decorators.
        /// </summary>
        Type ExportType { get; }

        /// <summary>
        /// Decorates the task execution and delegates the execution to the next decorator in <paramref name="decoratorQueue"/>
        /// </summary>
        /// <param name="decoratorQueue">The queue that contains the remaining decorators that will be executed</param>
        /// <param name="task">The actual task to execute</param>
        /// <param name="input">The input buffer for the task to execute</param>
        /// <param name="output">The output buffer for the task to execute</param>
        /// <param name="executionContext">The execution context for the task to execute</param>
        void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}