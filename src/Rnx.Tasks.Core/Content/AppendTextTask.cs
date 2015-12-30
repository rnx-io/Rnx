using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Content
{
    public class AppendTextTaskDescriptor : TaskDescriptorBase<AppendTextTask>
    {
        internal string TextToAppend { get; }

        public AppendTextTaskDescriptor(string textToAppend)
        {
            TextToAppend = textToAppend;
        }
    }

    public class AppendTextTask : RnxTask
    {
        private readonly AppendTextTaskDescriptor _appendTaskDescriptor;

        public AppendTextTask(AppendTextTaskDescriptor appendTaskDescriptor)
        {
            _appendTaskDescriptor = appendTaskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach (var e in input.Elements)
            {
                e.Text += _appendTaskDescriptor.TextToAppend;
                output.Add(e);
            }
        }
    }
}