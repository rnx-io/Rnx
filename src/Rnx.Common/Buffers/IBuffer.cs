using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Buffers
{
    public interface IBuffer : IDisposable
    {
        event EventHandler Ready;

        IEnumerable<IBufferElement> Elements { get; }
        Partitioner<IBufferElement> ElementsPartitioner { get; }

        void Add(IBufferElement item);
        void CompleteAdding();
        void CopyTo(IBuffer targetBuffer);
    }
}