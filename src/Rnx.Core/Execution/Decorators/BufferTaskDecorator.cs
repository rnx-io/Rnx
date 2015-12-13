using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;

namespace Rnx.Core.Execution.Decorators
{
    /// <summary>
    /// Makes sure that all remaining elements of the input buffer are copied to the output buffer.
    /// Also makes sure, that the CompleteAdding-method of the output buffer is called, to signal
    /// subsequent pipeline stages that all elements are in their input buffer.
    /// </summary>
    public class BufferTaskDecorator : AbstractTaskDecorator
    {
        public override void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            decoratorQueue.GetNext()?.Execute(decoratorQueue, task, input, output, executionContext);
            
            // make sure that remaining elements of the input buffer are copied to the output buffer
            input?.CopyTo(output);

            // make sure that this is called, otherwise the next pipeline stage would wait forever
            output?.CompleteAdding();
        }
    }
}