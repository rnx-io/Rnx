using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Common.Buffers;
using Rnx.Common.Execution;

namespace Rnx.Core.Tasks
{
    internal class UserDefinedTask : RnxTask
    {
        private string _taskName;
        private ITask _task;

        public UserDefinedTask(string taskName, ITask task)
        {
            _taskName = taskName;
            _task = task;
        }

        public override string Name => _taskName;

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            ExecuteTask(_task, input, output, executionContext);
        }
    }
}
