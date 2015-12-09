using Rnx.Common.Buffers;
using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Execution.Decorators
{
    public interface ITaskExecutionDecorator
    {
        Type ExportType { get; }
        ITaskExecutionDecorator Decoratee { set; }
        void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}