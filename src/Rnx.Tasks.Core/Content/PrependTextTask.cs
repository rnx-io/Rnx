using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;

namespace Rnx.Tasks.Core.Content
{
    public class PrependTextTaskDescriptor : TaskDescriptorBase<PrependTextTask>
    {
        public string TextToPrepend;

        public PrependTextTaskDescriptor(string textToPrepend)
        {
            TextToPrepend = textToPrepend;
        }
    }

    public class PrependTextTask : RnxTask
    {
        private readonly PrependTextTaskDescriptor _prependTaskDescriptor;

        public PrependTextTask(PrependTextTaskDescriptor prependTaskDescriptor)
        {
            _prependTaskDescriptor = prependTaskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Text = _prependTaskDescriptor.TextToPrepend + e.Text;
                output.Add(e);
            }
        }
    }
}