using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Buffers
{
    /// <summary>
    /// Null-Implementation for IBuffer interface. Mostly used for testing, but
    /// can also be helpful, when no actual buffer is needed, for example the input
    /// buffer of the first task, which is always empty
    /// </summary>
    public class NullBuffer : IBuffer
    {
        public event EventHandler Ready;
        public event EventHandler AddingComplete;
        public event EventHandler<IBufferElement> ElementAdded;

        public IEnumerable<IBufferElement> Elements => Enumerable.Empty<IBufferElement>();

        public Partitioner<IBufferElement> ElementsPartitioner => Partitioner.Create(Elements);

        public void Add(IBufferElement item)
        {
            ElementAdded?.Invoke(this, item);
    }

        public void CompleteAdding()
        {
            Ready?.Invoke(this, EventArgs.Empty);
            AddingComplete?.Invoke(this, EventArgs.Empty);
        }

        public void CopyTo(IBuffer targetBuffer)
        {
            foreach (var e in Elements)
            {
                targetBuffer?.Add(e);
            }
        }

        public void Dispose()
        { }
    }
}
