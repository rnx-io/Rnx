using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Represents a named user-defined task. Unique naming of user-defined tasks is required.
    /// </summary>
    public sealed class UserDefinedTask : RnxTask
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