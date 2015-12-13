using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Content.Internal
{
    public class ConcatTaskInternal : ConcatTask
    {
        private readonly IBufferElementFactory _bufferElementFactory;

        public ConcatTaskInternal(IBufferElementFactory bufferElementFactory, string targetFilepath, string separator = "")
            : base(targetFilepath, separator)
        {
            _bufferElementFactory = bufferElementFactory;
        }

        protected override IBufferElementFactory GetBufferElementFactory(IExecutionContext ctx) => _bufferElementFactory;
    }
}