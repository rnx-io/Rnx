using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Control
{
    public abstract class ControlTask : RnxTask
    {
        public ControlTask()
        {
            TaskDecorators = Enumerable.Repeat(new NullLoggingTaskDecorator(), 1);
        }
    }
}