using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Util;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Convinient base class for tasks
    /// </summary>
    public abstract class RnxTask : ITask
    {
        public abstract void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext);

        public virtual string Name => GetType().Name;
        public override string ToString() => Name;
        
        /// <summary>
        /// Shortcut to get the respective service from the <paramref name="executionContext"/> parameter
        /// </summary>
        /// <typeparam name="T">The type of the service</typeparam>
        protected virtual T GetService<T>(IExecutionContext executionContext)
        {
            return (T)executionContext.ServiceProvider.GetService(typeof(T));
        }

        protected virtual T RequireService<T>(IExecutionContext executionContext)
        {
            var service = executionContext.ServiceProvider.GetService(typeof(T));

            if(service == null)
            {
                throw new NullReferenceException($"Required service of type {typeof(T)} could not be found.");
            }

            return (T)service;
        }

        /// <summary>
        /// Helper method to execute a task. Remember: A task is never directly executed through its Execute-method.
        /// A task is always executed through an <see cref="ITaskExecuter"/>, because this will respect all
        /// decorators that are registered. A direct call to task.Execute would circumvent this logic.
        /// </summary>
        /// <param name="task">The task that should be executed</param>
        /// <param name="input">The input buffer for the task</param>
        /// <param name="output">The output buffer for the task</param>
        /// <param name="executionContext">The execution context for the task</param>
        protected void ExecuteTask(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var taskExecuter = RequireService<ITaskExecuter>(executionContext);
            taskExecuter.Execute(task, input, output, executionContext);
        }

        protected virtual ITaskExecuter GetTaskExecuter(IExecutionContext ctx) => RequireService<ITaskExecuter>(ctx);
    }
}