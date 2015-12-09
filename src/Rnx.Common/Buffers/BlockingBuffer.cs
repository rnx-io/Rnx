using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Buffers
{
    public class BlockingBuffer : IBuffer
    {
        public event EventHandler Ready;

        private BlockingCollection<IBufferElement> _blockingCollection;
        private bool _isReady = false;

        public BlockingBuffer()
        {
            _blockingCollection = new BlockingCollection<IBufferElement>();
        }

        public IEnumerable<IBufferElement> Elements => _blockingCollection.GetConsumingEnumerable();

        public Partitioner<IBufferElement> ElementsPartitioner => new BlockingCollectionPartitioner<IBufferElement>(_blockingCollection);

        public void Add(IBufferElement element)
        {
            CheckReadiness();
            _blockingCollection.Add(element);
        }

        public void CompleteAdding()
        {
            CheckReadiness();
            _blockingCollection.CompleteAdding();
        }

        public void CopyTo(IBuffer targetBuffer)
        {
            if (targetBuffer != null)
            {
                foreach (var e in Elements)
                {
                    targetBuffer.Add(e);
                }
            }
        }

        public void Dispose()
        {
            _blockingCollection.Dispose();
        }

        private void CheckReadiness()
        {
            if (!_isReady)
            {
                _isReady = true;
                Ready?.Invoke(this, EventArgs.Empty);
            }
        }

        // Adapted from: http://blogs.msdn.com/b/pfxteam/archive/2010/04/06/9990420.aspx
        private class BlockingCollectionPartitioner<T> : Partitioner<T>
        {
            private BlockingCollection<T> _collection;

            public BlockingCollectionPartitioner(BlockingCollection<T> collection)
            {
                _collection = collection;
            }

            public override bool SupportsDynamicPartitions => true;

            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
            {
                var dynamicPartitioner = GetDynamicPartitions();

                return Enumerable.Range(0, partitionCount).Select(_ => dynamicPartitioner.GetEnumerator()).ToArray();
            }

            public override IEnumerable<T> GetDynamicPartitions() => _collection.GetConsumingEnumerable();
        }
    }
}
