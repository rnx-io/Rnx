using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Util
{
    public class ExecuteTaskDescriptor : TaskDescriptorBase<ExecuteTask>
    {
        internal Action<IBuffer,IBuffer,IExecutionContext> TaskAction { get; private set; }
        internal Action<IBufferElement> ElementAction { get; private set; }

        public ExecuteTaskDescriptor(Action<IBuffer, IBuffer, IExecutionContext> taskAction)
        {
            TaskAction = taskAction;
        }

        public ExecuteTaskDescriptor(Action<IBufferElement> elementAction)
        {
            ElementAction = elementAction;
        }
    }

    public class ExecuteTask : RnxTask
    {
        private readonly ExecuteTaskDescriptor _taskDescriptor;

        public ExecuteTask(ExecuteTaskDescriptor taskDescriptor)
        {
            _taskDescriptor = taskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            if(_taskDescriptor.TaskAction != null)
            {
                _taskDescriptor.TaskAction(input, output, executionContext);
            }
            else if(_taskDescriptor.ElementAction != null)
            {
                foreach(var e in input.Elements)
                {
                    _taskDescriptor.ElementAction(e);
                    output.Add(e);
                }
            }
        }
    }
}