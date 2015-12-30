using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Control
{
    public class SeriesTaskDescriptor : MultiTaskDescriptor<SeriesTask>
    {
        public const int DEFAULT_MAX_DEGREE_OF_PARALLELISM = 8;

        internal int MaxDegreeOfParallelism { get; }

        public SeriesTaskDescriptor(int maxDegreeOfParallelism, params ITaskDescriptor[] taskDescriptors)
            : base(taskDescriptors)
        {
            if(maxDegreeOfParallelism < 1)
            {
                throw new ArgumentException($"Invalid value for {nameof(maxDegreeOfParallelism)}. Only positive values are allowed.", nameof(maxDegreeOfParallelism));
            }

            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public SeriesTaskDescriptor(params ITaskDescriptor[] taskDescriptors)
            : this(DEFAULT_MAX_DEGREE_OF_PARALLELISM, taskDescriptors)
        { }
    }

    public class SeriesTask : ControlTask
    {
        private readonly SeriesTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public SeriesTask(SeriesTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _taskDescriptor = taskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var numberOfTasks = _taskDescriptor.TaskDescriptors.Length;

            if (numberOfTasks == 1)
            {
                // special handling. avoid expensive semaphore/threading stuff
                _taskExecuter.Execute(_taskDescriptor.TaskDescriptors.First(), input, output, executionContext);
                return;
            }

            var numberOfConcurrentTasks = Math.Min(_taskDescriptor.TaskDescriptors.Length, _taskDescriptor.MaxDegreeOfParallelism);
            var slimSemaphore = new SemaphoreSlim(numberOfConcurrentTasks);
            Action firstAction = null;
            var subsequentActions = new BlockingCollection<Action>();
            var runningTasks = new List<Task>();
            var buffers = new List<IBuffer>();
            buffers.Add(input);

            for (int i = 0; i < numberOfTasks; ++i)
            {
                var taskDescriptor = _taskDescriptor.TaskDescriptors[i];

                // input buffer of current task is the output buffer of the last task
                var taskInputBuffer = buffers.Last();

                // if we are at the last task, use the incoming output buffer, otherwise create a new buffer
                var taskOutputBuffer = i == (numberOfTasks - 1) ? output : _bufferFactory.Create();
                Action executeAction = () =>
                {
                    _taskExecuter.Execute(taskDescriptor, taskInputBuffer, taskOutputBuffer, executionContext);

                    // release semaphore to signal that a slot is free for another task
                    slimSemaphore.Release();
                };

                if (i == 0)
                {
                    firstAction = executeAction;
                }
                else
                {
                    var isLastTask = (i == (numberOfTasks - 1));
                    EventHandler eventAction = (s, e) =>
                    {
                        // if a slot is free it will be processed right away, otherwise the execution will be delayed until
                        // a slot is free
                        subsequentActions.Add(executeAction);

                        if (isLastTask)
                        {
                            // signal that no more subsequent actions will be added
                            subsequentActions.CompleteAdding();
                        }
                    };

                    if (taskDescriptor.RequiresCompletedInputBuffer)
                    {
                        // the execution of the subsequent action will only then triggered, when all elements of the input buffer were processed
                        taskInputBuffer.AddingComplete += eventAction;
                    }
                    else
                    {
                        // if at least one item was processed in the previous buffer ("Ready" event), we're ready to add the next subsequent action.
                        taskInputBuffer.Ready += eventAction;
                    }
                }

                buffers.Add(taskOutputBuffer);
            }

            // call first Wait to signal that 1 slot is used by the first action
            slimSemaphore.Wait();

            // run initial task
            runningTasks.Add(Task.Run(firstAction));

            foreach (var subsequentAction in subsequentActions.GetConsumingEnumerable())
            {
                // blocks when the maximum number of concurrent actions is reached
                slimSemaphore.Wait();

                // run next task
                runningTasks.Add(Task.Run(subsequentAction));
            }

            // cleanup
            subsequentActions.Dispose();

            // wait for all tasks to complete
            Task.WaitAll(runningTasks.ToArray());

            // cleanup after all tasks are completed
            slimSemaphore.Dispose();

            // dispose only the created buffers (i.e. do not dispose the incoming input and output buffers)
            foreach (var buffer in buffers.Where(f => f != input && f != output))
            {
                buffer.Dispose();
            }
        }
    }
}