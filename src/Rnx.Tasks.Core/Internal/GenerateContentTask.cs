using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Internal
{
    public class GenerateContentTaskDescriptor : TaskDescriptorBase<GenerateContentTask>
    {
        internal int NumberOfElements { get; }
        internal Func<int, string> TextGenerator { get; }

        public GenerateContentTaskDescriptor(int numberOfElements, Func<int, string> textGenerator = null)
        {
            NumberOfElements = numberOfElements;
            TextGenerator = textGenerator ?? (f => f.ToString());
        }
    }

    public class GenerateContentTask : RnxTask
    {
        private readonly GenerateContentTaskDescriptor _creationTaskDescriptor;
        private readonly IBufferElementFactory _bufferElementFactory;

        public GenerateContentTask(GenerateContentTaskDescriptor creationTaskDescriptor, IBufferElementFactory bufferElementFactory)
        {
            _creationTaskDescriptor = creationTaskDescriptor;
            _bufferElementFactory = bufferElementFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            for (int i = 0; i < _creationTaskDescriptor.NumberOfElements; ++i)
            {
                output.Add(_bufferElementFactory.Create(_creationTaskDescriptor.TextGenerator(i)));
            }

            output.CompleteAdding();
        }
    }
}