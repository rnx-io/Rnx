using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// NoOp-task descriptor
    /// </summary>
    public class NullTaskDescriptor : TaskDescriptorBase<NullTask>
    {}

    /// <summary>
    /// NoOp-task that only copies the incoming input buffer to the output bufffer
    /// </summary>
    public class NullTask : RnxTask
    {
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            input?.CopyTo(output);
            output?.CompleteAdding();
        }
    }
}