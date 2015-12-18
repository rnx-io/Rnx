using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Control
{
    public class IfTaskDescriptor : TaskDescriptorBase<IfTask>
    {
        internal List<PredicateTaskDescriptorPair> PredicateTaskPairs { get; } = new List<PredicateTaskDescriptorPair>();
        private bool _elseCalled;

        public IfTaskDescriptor(Predicate<IBufferElement> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
        }

        public IfTaskDescriptor ElseIf(Predicate<IBufferElement> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            CheckElseCalled();

            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
            return this;
        }

        public IfTaskDescriptor Else(ITaskDescriptor taskDescriptorToRun)
        {
            CheckElseCalled();

            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(t => true, taskDescriptorToRun));
            _elseCalled = true;
            return this;
        }

        private void CheckElseCalled()
        {
            if (_elseCalled)
            {
                throw new InvalidOperationException("Invalid operation. No more tasks possible after the Else-method was called.");
            }
        }
    }

    public class IfTask : ControlTask
    {
        private readonly IfTaskDescriptor _ifTaskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public IfTask(IfTaskDescriptor ifTaskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _ifTaskDescriptor = ifTaskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }
        
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferChainer = new BufferFactoryChainDecorator(_bufferFactory, output); // connects the created buffers to the output buffer
            var taskInputBufferMap = new Dictionary<ITaskDescriptor, IBuffer>();
            var runningTasks = new List<Task>();

            foreach (var e in input.Elements)
            {
                var matchingPredicatePair = _ifTaskDescriptor.PredicateTaskPairs.FirstOrDefault(f => f.Item1(e));

                if (matchingPredicatePair != null)
                {
                    IBuffer existingTaskInputBuffer;

                    if (taskInputBufferMap.TryGetValue(matchingPredicatePair.Item2, out existingTaskInputBuffer))
                    {
                        // A matching task was already started. Add the current element to the according input buffer of this task
                        existingTaskInputBuffer.Add(e);
                    }
                    else
                    {
                        var taskToRunInputBuffer = _bufferFactory.Create();
                        taskToRunInputBuffer.Add(e);
                        taskInputBufferMap.Add(matchingPredicatePair.Item2, taskToRunInputBuffer);

                        var runningTask = Task.Run(() =>
                        {
                            _taskExecuter.Execute(matchingPredicatePair.Item2, taskToRunInputBuffer, bufferChainer.Create(), executionContext);
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

    internal class PredicateTaskDescriptorPair : Tuple<Predicate<IBufferElement>, ITaskDescriptor>
    {
        public PredicateTaskDescriptorPair(Predicate<IBufferElement> item1, ITaskDescriptor item2)
            : base(item1, item2)
        { }
    }
}