using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Control
{
    public class ParallelTaskDescriptor : MultiTaskDescriptor<ParallelTask>
    {
        internal ParallelTaskOutputStrategy OutputStrategy { get; }

        public ParallelTaskDescriptor(ParallelTaskOutputStrategy outputStrategy, params ITaskDescriptor[] taskDescriptors)
            : base(taskDescriptors)
        {
            OutputStrategy = outputStrategy;
        }
    }

    public class ParallelTask : ControlTask
    {
        private readonly ParallelTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public ParallelTask(ParallelTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _taskDescriptor = taskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            // for every sub-task create a new buffer, which will be filled later when new elements arrive
            var taskInputBufferMap = _taskDescriptor.TaskDescriptors.ToDictionary(f => f, f => _bufferFactory.Create());

            // remember all created output buffers, so we can dispose them later
            var createdTaskOutputBuffers = new List<IBuffer>();

            // fire up all tasks in parallel
            var parallelTasks = _taskDescriptor.TaskDescriptors.Select(d =>
            {
                IBuffer taskOutputBuffer;

                // if current input should go to output without any processing, set the output buffer of the sub-tasks to a NullBuffer
                if (_taskDescriptor.OutputStrategy == ParallelTaskOutputStrategy.DoNotChangeInput)
                {
                    taskOutputBuffer = new NullBuffer();
                }
                else
                {
                    taskOutputBuffer = _bufferFactory.Create();

                    // chain the output buffer of this task to the global output buffer
                    taskOutputBuffer.ElementAdded += (s, e) => output.Add(e);
                }

                createdTaskOutputBuffers.Add(taskOutputBuffer);

                return Task.Run(() => _taskExecuter.Execute(d, taskInputBufferMap[d], taskOutputBuffer, executionContext));
            }).ToArray();

            // iterate through all elements from the input buffer and copy them to the output buffer.
            // also add the clone of each element to the inputBuffer of the async executing task
            foreach (var e in input.Elements)
            {
                if (_taskDescriptor.OutputStrategy != ParallelTaskOutputStrategy.ReplaceInput)
                {
                    output.Add(e);
                }

                foreach (var b in taskInputBufferMap.Values)
                {
                    b.Add(e.Clone());
                }
            }

            // after iteration through input buffer is done, notify 
            foreach (var buffer in taskInputBufferMap.Values)
            {
                buffer.CompleteAdding();
            }

            // Wait for the parallel tasks to complete
            Task.WaitAll(parallelTasks);

            // dispose all created input and output buffers
            foreach (var buffer in taskInputBufferMap.Values.Concat(createdTaskOutputBuffers))
            {
                buffer.Dispose();
            }
        }
    }

    /// <summary>
    /// Specifies how parallel running tasks should modify the output buffer
    /// </summary>
    public enum ParallelTaskOutputStrategy
    {
        /// <summary>
        /// Specifies that the output of the parallel running tasks should be concated to the current input
        /// </summary>
        ConcatToInput = 0,
        /// <summary>
        /// Specifies that the parallel running tasks should not be able to push elements to the output buffer
        /// </summary>
        DoNotChangeInput,
        /// <summary>
        /// The current input buffer will be discarded and replaced with the elements that are produced by the parallel running tasks
        /// </summary>
        ReplaceInput
    }
}