using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Common.Buffers;
using Rnx.Common.Tasks;

namespace Rnx.Common.Execution.Decorators
{
    public abstract class AbstractTaskExecutionDecorator : ITaskExecutionDecorator
    {
        public ITaskExecutionDecorator Decoratee { protected get; set; }

        public virtual Type ExportType => GetType();

        public abstract void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}
