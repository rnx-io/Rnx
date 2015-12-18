using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;

namespace Rnx.Tasks.Core.Content
{
    public class ReplaceTextTaskDescriptor : TaskDescriptorBase<ReplaceTextTask>
    {
        public string SearchText { get; }
        public string Replacement { get; }

        public ReplaceTextTaskDescriptor(string searchText, string replacement)
        {
            SearchText = searchText;
            Replacement = replacement;
        }
    }

    public class ReplaceTextTask : RnxTask
    {
        private readonly ReplaceTextTaskDescriptor _replaceTaskDescriptor;

        public ReplaceTextTask(ReplaceTextTaskDescriptor replaceTaskDescriptor)
        {
            _replaceTaskDescriptor = replaceTaskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Text = e.Text.Replace(_replaceTaskDescriptor.SearchText, _replaceTaskDescriptor.Replacement);
                output.Add(e);
            }
        }
    }
}