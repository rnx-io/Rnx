using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Exceptions;

namespace Rnx.Tasks.Core.Content
{
    public class ReplaceTextTaskDescriptor : TaskDescriptorBase<ReplaceTextTask>
    {
        internal string SearchText { get; }
        internal string Replacement { get; }
        internal ITaskDescriptor ReplacementProvidingTaskDescriptor { get; }

        public ReplaceTextTaskDescriptor(string searchText, string replacement)
        {
            SearchText = searchText;
            Replacement = replacement;
        }

        public ReplaceTextTaskDescriptor(string searchText, ITaskDescriptor replacementProvidingTaskDescriptor)
        {
            SearchText = searchText;
            ReplacementProvidingTaskDescriptor = replacementProvidingTaskDescriptor;
        }
    }

    public class ReplaceTextTask : RnxTask
    {
        private readonly ReplaceTextTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public ReplaceTextTask(ReplaceTextTaskDescriptor replaceTaskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _taskDescriptor = replaceTaskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var replacement = _taskDescriptor.Replacement;

            if(_taskDescriptor.ReplacementProvidingTaskDescriptor != null)
            {
                using (var outputBuffer = _bufferFactory.Create())
                {
                    _taskExecuter.Execute(_taskDescriptor.ReplacementProvidingTaskDescriptor, new NullBuffer(), outputBuffer, executionContext);
                    var replacementProvidingElement = outputBuffer.Elements.FirstOrDefault();

                    if (replacementProvidingElement == null)
                    {
                        throw new RnxException($"Invalid task descriptor. The provided task descriptor for {nameof(ReplaceTextTask)} hasn't yielded any elements.");
                    }

                    replacement = replacementProvidingElement.Text;
                }
            }

            foreach (var e in input.Elements)
            {
                e.Text = e.Text.Replace(_taskDescriptor.SearchText, replacement ?? "");
                output.Add(e);
            }
        }
    }
}