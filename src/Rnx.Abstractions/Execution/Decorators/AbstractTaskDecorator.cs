using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Tasks;

namespace Rnx.Abstractions.Execution.Decorators
{
    /// <summary>
    /// Abstract base class for <see cref="ITaskDecorator"/>s
    /// </summary>
    public abstract class AbstractTaskDecorator : ITaskDecorator
    {
        public virtual Type ExportType => GetType();

        public abstract void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}