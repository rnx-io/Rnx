using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Control
{
    public class BlockWiseData
    {
        public int BlockSize { get; }
        public int CurrentBlockIndex { get; }
        public int? TotalBlocksCount { get; private set; }

        public bool HasPrevious => CurrentBlockIndex > 1;
        public bool HasNext => CurrentBlockIndex < TotalBlocksCount;

        public BlockWiseData(int blockSize, int currentBlockIndex, int? totalBlocksCount)
        {
            if (blockSize < 1)
            {
                throw new ArgumentException(nameof(blockSize));
            }

            if (currentBlockIndex < 1)
            {
                throw new ArgumentException(nameof(currentBlockIndex));
            }

            if(totalBlocksCount.HasValue && totalBlocksCount.Value < 1)
            {
                throw new ArgumentException(nameof(totalBlocksCount));
            }

            BlockSize = blockSize;
            CurrentBlockIndex = currentBlockIndex;
            TotalBlocksCount = totalBlocksCount;
        }
    }
}