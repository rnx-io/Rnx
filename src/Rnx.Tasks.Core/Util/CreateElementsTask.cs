using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Util
{
    public class CreateElementsTaskDescriptor : TaskDescriptorBase<CreateElementsTask>
    {
        internal string[] ElementTexts { get; }

        public CreateElementsTaskDescriptor(params string[] elementTexts)
        {
            ElementTexts = elementTexts;
        }
    }

    public class CreateElementsTask : RnxTask
    {
        private readonly CreateElementsTaskDescriptor _taskDescriptor;
        private readonly IBufferElementFactory _bufferElementFactory;

        public CreateElementsTask(CreateElementsTaskDescriptor taskDescriptor, IBufferElementFactory bufferElementFactory)
        {
            _taskDescriptor = taskDescriptor;
            _bufferElementFactory = bufferElementFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var text in _taskDescriptor.ElementTexts)
            {
                var newElement = _bufferElementFactory.Create(text);
                output.Add(newElement);
            }
        }
    }
}