using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Internal
{
    internal class PredicateTaskDescriptorPairExecuter
    {
        private readonly IEnumerable<PredicateTaskDescriptorPair> _predicateTaskPairs;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public PredicateTaskDescriptorPairExecuter(IEnumerable<PredicateTaskDescriptorPair> predicateTaskPairs, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _predicateTaskPairs = predicateTaskPairs;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        public void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferChainer = new BufferFactoryChainDecorator(_bufferFactory, output); // connects the created buffers to the output buffer
            var taskInputBufferMap = new Dictionary<ITaskDescriptor, IBuffer>();
            var runningTasks = new List<Task>();

            foreach (var e in input.Elements)
            {
                var matchingPredicatePair = _predicateTaskPairs.FirstOrDefault(f => f.Predicate(e));

                if (matchingPredicatePair != null)
                {
                    IBuffer existingTaskInputBuffer;

                    if (taskInputBufferMap.TryGetValue(matchingPredicatePair.TaskDescriptor, out existingTaskInputBuffer))
                    {
                        // A matching task was already started. Add the current element to the according input buffer of this task
                        existingTaskInputBuffer.Add(e);
                    }
                    else
                    {
                        var taskToRunInputBuffer = _bufferFactory.Create();
                        taskToRunInputBuffer.Add(e);
                        taskInputBufferMap.Add(matchingPredicatePair.TaskDescriptor, taskToRunInputBuffer);

                        var runningTask = Task.Run(() =>
                        {
                            _taskExecuter.Execute(matchingPredicatePair.TaskDescriptor, taskToRunInputBuffer, bufferChainer.Create(), executionContext);
                        });
                        runningTasks.Add(runningTask);
                    }
                }
                else
                {
                    // No matching predicate - push unchanged element to the next stage
                    output.Add(e);
                }
            }

            // Signal all input buffers that we're done adding elements
            foreach (var inputBuffer in taskInputBufferMap.Values)
            {
                inputBuffer.CompleteAdding();
            }

            // Wait for tasks to complete
            Task.WaitAll(runningTasks.ToArray());

            // dispose all created output buffers
            bufferChainer.Dispose();

            // dispose all created input buffers
            foreach (var inputBuffer in taskInputBufferMap.Values)
            {
                inputBuffer.Dispose();
            }
        }
    }
}
