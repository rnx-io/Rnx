using Rnx.Common.Buffers;
using Rnx.Common.Tasks;

namespace Rnx.Common.Execution.Decorators
{
    public class NullLoggingExecutionDecorator : AbstractLoggingExecutionDecorator
    {
        public override void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            Decoratee.Execute(task, input, output, executionContext);
        }
    }
}