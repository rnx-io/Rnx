using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Buffers
{
    public class NullBuffer : IBuffer
    {
        public int Count => 0;

        public event EventHandler Ready;
        public event EventHandler<IBufferElement> ElementAdded;

        public IEnumerable<IBufferElement> Elements => Enumerable.Empty<IBufferElement>();

        public Partitioner<IBufferElement> ElementsPartitioner => null;

        public void Add(IBufferElement item)
        {
            ElementAdded?.Invoke(this, item);
        }

        public void CompleteAdding()
        {
            Ready?.Invoke(this, EventArgs.Empty);
        }

        public void CopyTo(IBuffer targetBuffer)
        { }

        public void Dispose()
        { }
    }
}
