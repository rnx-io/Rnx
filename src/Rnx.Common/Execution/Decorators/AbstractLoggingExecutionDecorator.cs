using Rnx.Common.Buffers;
using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Execution.Decorators
{
    public abstract class AbstractLoggingExecutionDecorator : AbstractTaskExecutionDecorator, ILoggingExecutionDecorator
    {
        public override Type ExportType => typeof(ILoggingExecutionDecorator);
    }
}