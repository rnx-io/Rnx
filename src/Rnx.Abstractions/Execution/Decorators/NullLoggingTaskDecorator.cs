using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Tasks;

namespace Rnx.Abstractions.Execution.Decorators
{
    /// <summary>
    /// Can be used to disable the logging decorator for a specific task.
    /// This addresses only the decorator. All other logging inside of that task will not be affected.
    /// </summary>
    public class NullLoggingTaskDecorator : AbstractLoggingTaskDecorator
    {
        public override void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            decoratorQueue.GetNext()?.Execute(decoratorQueue, task, input, output, executionContext);
        }
    }
}