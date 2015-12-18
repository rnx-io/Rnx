
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Tasks;
using System.Collections.Generic;

namespace Rnx.Abstractions.Execution
{
    /// <summary>
    /// Responsible for executing a task. Every task must be executed via ITaskExecuter.
    /// </summary>
    public interface ITaskExecuter
    {
        /// <summary>
        /// Executes the given task with the provided parameters
        /// </summary>
        /// <param name="taskDescriptor">The task descriptor describing the task that should be executed</param>
        /// <param name="input">The input buffer for the task</param>
        /// <param name="output">The output buffer for the task</param>
        /// <param name="executionContext">The execution context for the task</param>
        void Execute(ITaskDescriptor taskDescriptor, IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}