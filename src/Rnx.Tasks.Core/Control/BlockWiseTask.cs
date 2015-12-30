using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Tasks.Core.Internal;

namespace Rnx.Tasks.Core.Control
{
    public class BlockWiseTaskDescriptor : TaskDescriptorBase<BlockWiseTask>
    {
        internal int BlockSize { get; }
        internal bool AllowParallelExecutionOfBlocks { get; private set; }
        internal Func<BlockWiseData,ITaskDescriptor> TaskDescriptorToRun { get; }

        public BlockWiseTaskDescriptor(int blockSize, Func<BlockWiseData, ITaskDescriptor> taskDescriptorToRun, bool requiresDetailedBlockInfo = false)
        {
            if(blockSize <= 0)
            {
                throw new ArgumentException(nameof(blockSize));
            }

            if(taskDescriptorToRun == null)
            {
                throw new ArgumentException(nameof(taskDescriptorToRun));
            }

            TaskDescriptorToRun = taskDescriptorToRun;
            BlockSize = blockSize;
            RequiresCompletedInputBuffer = requiresDetailedBlockInfo;
        }

        public BlockWiseTaskDescriptor AllowParallelExecution(bool value = true)
        {
            AllowParallelExecutionOfBlocks = value;
            return this;
        }
    }

    public class BlockWiseTask : ControlTask
    {
        private readonly BlockWiseTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;
        private readonly IProcessingStrategy _processingStrategy;

        public BlockWiseTask(BlockWiseTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _taskDescriptor = taskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
            _processingStrategy = _taskDescriptor.AllowParallelExecutionOfBlocks ? (IProcessingStrategy)new ParallelProcessingStrategy() : new NormalProcessingStrategy();
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var elementsToProcess = new List<IBufferElement>();
            var count = 0;
            var currentBlockIndex = 1;
            var elements = input.Elements;
            Func<int, BlockWiseData> blockWiseDataFactory;
            var createdInputBuffers = new List<IBuffer>();
            var bufferChainer = new BufferFactoryChainDecorator(_bufferFactory, output); // connects the created buffers to the tempOutputBuffer

            if (_taskDescriptor.RequiresCompletedInputBuffer)
            {
                var arr = elements.ToArray(); // this will block until all elements are in the buffer
                elements = arr;
                blockWiseDataFactory = currentIndex => new BlockWiseData(_taskDescriptor.BlockSize, currentIndex, arr.Length);
            }
            else
            {
                blockWiseDataFactory = currentIndex => new BlockWiseData(_taskDescriptor.BlockSize, currentIndex, null);
            }

            foreach(var e in elements)
            {
                if(count > 0 && count % _taskDescriptor.BlockSize == 0)
                {
                    var createdInputBuffer = ProcessElements(blockWiseDataFactory(currentBlockIndex), elementsToProcess, bufferChainer.Create(), executionContext);
                    createdInputBuffers.Add(createdInputBuffer);
                    currentBlockIndex++;
                }

                elementsToProcess.Add(e);
                count++;
            }

            if(elementsToProcess.Any())
            {
                var createdInputBuffer = ProcessElements(blockWiseDataFactory(currentBlockIndex), elementsToProcess, bufferChainer.Create(), executionContext);
                createdInputBuffers.Add(createdInputBuffer);
            }

            _processingStrategy.WaitForCompletion();

            // dispose all created input buffers
            foreach(var buffer in createdInputBuffers)
            {
                buffer.Dispose();
            }

            // dispose all created output buffers
            bufferChainer.Dispose();
        }

        private IBuffer ProcessElements(BlockWiseData blockWiseData, List<IBufferElement> elementsToProcess, IBuffer output, IExecutionContext executionContext)
        {
            var inputBuffer = _bufferFactory.Create();

            foreach (var blockBufferElement in elementsToProcess)
            {
                inputBuffer.Add(blockBufferElement);
            }

            inputBuffer.CompleteAdding();

            _processingStrategy.Run(() => _taskExecuter.Execute(_taskDescriptor.TaskDescriptorToRun(blockWiseData), inputBuffer, output, executionContext));

            elementsToProcess.Clear();

            return inputBuffer;
        }

        private interface IProcessingStrategy
        {
            void Run(Action action);
            void WaitForCompletion();
        }

        private class NormalProcessingStrategy : IProcessingStrategy
        {
            public void Run(Action action)
            {
                action();
            }

            public void WaitForCompletion()
            {}
        }

        private class ParallelProcessingStrategy : IProcessingStrategy
        {
            private readonly List<Task> _tasks = new List<Task>();

            public void Run(Action action)
            {
                var task = Task.Run(action);
                _tasks.Add(task);
            }

            public void WaitForCompletion()
            {
                Task.WaitAll(_tasks.ToArray());
                _tasks.Clear();
            }
        }
    }
}