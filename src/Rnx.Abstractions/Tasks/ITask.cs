using System.Collections.Generic;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Represents the interface that all tasks must implement to be runnable via Rnx
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Name of the task
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes the main logic for this task. The <paramref name="input"/> buffer contains
        /// all incoming elements that can be used for processing. Any element that should go into
        /// the next processing stage, must be added to the <paramref name="output"/> buffer.
        /// </summary>
        /// <param name="input">The input buffer containing the elements from the previous processing stage</param>
        /// <param name="output">The output buffer is the input buffer of the next processing stage. Any processed element
        /// that should go into the next processing stage must be added to this buffer.</param>
        /// <param name="executionContext">Some context information that might be helpful for performing a task</param>
        void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}