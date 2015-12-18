using Rnx.Abstractions.Buffers;
using System;
using System.Collections.Generic;

namespace Rnx.Tasks.Core.Internal
{
    public class BufferFactoryChainDecorator : IBufferFactory, IDisposable
    {
        private readonly IBufferFactory _bufferFactory;
        private readonly IBuffer _bufferToChainTo;
        private readonly List<IBuffer> _createdBuffers = new List<IBuffer>();

        public BufferFactoryChainDecorator(IBufferFactory bufferFactory, IBuffer bufferToChainTo)
        {
            _bufferFactory = bufferFactory;
            _bufferToChainTo = bufferToChainTo;
        }

        public IBuffer Create()
        {
            var buffer = _bufferFactory.Create();
            buffer.ElementAdded += (s, e) => _bufferToChainTo.Add(e);
            _createdBuffers.Add(buffer);

            return buffer;
        }

        public void Dispose()
        {
            foreach (var buffer in _createdBuffers)
            {
                buffer.Dispose();
            }

            _createdBuffers.Clear();
        }
    }
}