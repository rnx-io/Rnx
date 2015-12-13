using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Control
{
    public class IfTask : RnxTask, ITaskDecorationProvider
    {
        private readonly List<PredicateTaskPair> _predicateTaskPairs;

        public IEnumerable<ITaskDecorator> TaskDecorators
        {
            get
            {
                yield return new NullLoggingTaskDecorator();
            }
        }

        public IfTask(Predicate<IBufferElement> predicate, ITask taskToRun)
        {
            _predicateTaskPairs = new List<PredicateTaskPair>();
            _predicateTaskPairs.Add(new PredicateTaskPair(predicate, taskToRun));
        }

        public IfTask ElseIf(Predicate<IBufferElement> predicate, ITask taskToRun)
        {
            _predicateTaskPairs.Add(new PredicateTaskPair(predicate, taskToRun));
            return this;
        }

        public IfTask Else(ITask taskToRun)
        {
            _predicateTaskPairs.Add(new PredicateTaskPair(t => true, taskToRun));
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferFactory = GetBufferFactory(executionContext);
            var taskExecuter = GetTaskExecuter(executionContext);

            var taskInputBufferMap = new Dictionary<ITask, IBuffer>();
            var runningTasks = new List<Task>();

            foreach (var e in input.Elements)
            {
                var matchingPredicatePair = _predicateTaskPairs.FirstOrDefault(f => f.Item1(e));

                if (matchingPredicatePair != null)
                {
                    IBuffer existingTaskBuffer;

                    if (taskInputBufferMap.TryGetValue(matchingPredicatePair.Item2, out existingTaskBuffer))
                    {
                        // A matching task was already started. Add the current element to the according input buffer of this task
                        existingTaskBuffer.Add(e);
                    }
                    else
                    {
                        var taskToRunInputBuffer = bufferFactory.Create();
                        taskToRunInputBuffer.Add(e);
                        taskInputBufferMap.Add(matchingPredicatePair.Item2, taskToRunInputBuffer);

                        var runningTask = Task.Run(() =>
                        {
                            taskExecuter.Execute(matchingPredicatePair.Item2, taskToRunInputBuffer, output, executionContext);
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

            // dispose all created input buffers
            foreach (var inputBuffer in taskInputBufferMap.Values)
            {
                inputBuffer.Dispose();
            }
        }

        protected virtual IBufferFactory GetBufferFactory(IExecutionContext ctx) => RequireService<IBufferFactory>(ctx);

        private class PredicateTaskPair : Tuple<Predicate<IBufferElement>, ITask>
        {
            public PredicateTaskPair(Predicate<IBufferElement> item1, ITask item2)
                : base(item1, item2)
            { }
        }
    }
}