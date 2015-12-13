using Rnx.Abstractions.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Buffers
{
    /// <summary>
    /// Default implementation of <see cref="IBufferFactory"/> to
    /// create new <see cref="BlockingBuffer"/> objects
    /// </summary>
    public class BlockingBufferFactory : IBufferFactory
    {
        public IBuffer Create()
        {
            return new BlockingBuffer();
        }
    }
}