using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Execution.Decorators;
using Rnx.Common.Tasks;

namespace Rnx.Core.Execution.Decorators
{
    public class BufferTaskExecutionDecorator : AbstractTaskExecutionDecorator
    {
        public override void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            Decoratee.Execute(task, input, output, executionContext);
            
            if(output != null)
            {
                // make sure that remaining elements of the input buffer are copied to the output buffer
                input?.CopyTo(output);

                // make sure that this is called, otherwise the next pipeline stage would wait forever
                output.CompleteAdding();
            }
        }
    }
}