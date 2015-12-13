using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Compression.Internal
{
    public class UnzipTaskInternal : UnzipTask
    {
        private readonly IBufferElementFactory _bufferElementFactory;

        public UnzipTaskInternal(IBufferElementFactory bufferElementFactory)
        {
            _bufferElementFactory = bufferElementFactory;
        }

        protected override IBufferElementFactory GetBufferElementFactory(IExecutionContext ctx) => _bufferElementFactory;
    }
}