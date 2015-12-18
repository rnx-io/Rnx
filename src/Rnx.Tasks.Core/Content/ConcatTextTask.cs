using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.FileSystem;
using System.Linq;
using System;

namespace Rnx.Tasks.Core.Content
{
    public class ConcatTextTaskDescriptor : TaskDescriptorBase<ConcatTextTask>
    {
        public string TargetFilepath { get; }
        public string Separator { get; }

        public ConcatTextTaskDescriptor(string targetFilepath, string separator = "")
        {
            TargetFilepath = targetFilepath;
            Separator = separator;
        }
    }

    public class ConcatTextTask : RnxTask
    {
        private readonly ConcatTextTaskDescriptor _taskDescriptor;
        private readonly IBufferElementFactory _bufferElementFactory;

        public ConcatTextTask(ConcatTextTaskDescriptor taskDescriptor, IBufferElementFactory bufferElementFactory)
        {
            _taskDescriptor = taskDescriptor;
            _bufferElementFactory = bufferElementFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var inputElements = input.Elements.ToArray();
            var concatedText = string.Join(_taskDescriptor.Separator, inputElements.Select(f => f.Text).ToArray());

            var newElement = _bufferElementFactory.Create(concatedText);
            newElement.Data.Add(new WriteFileData(_taskDescriptor.TargetFilepath));
            output.Add(newElement);
        }
    }
}