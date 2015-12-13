using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Composite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Composite
{
    public class ParallelTask : MultiTask
    {
        private bool _requiresClonedElements;

        public ParallelTask(params ITask[] tasks)
            : this(false, tasks)
        { }

        public ParallelTask(bool requiresClonedElements, params ITask[] tasks)
            : base(tasks)
        {
            _requiresClonedElements = requiresClonedElements;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferFactory = GetBufferFactory(executionContext);
            var taskExecuter = GetTaskExecuter(executionContext);

            IBuffer nullBuffer = new NullBuffer();
            var taskInputBufferMap = Tasks.ToDictionary(f => f, f => bufferFactory.Create());
            var parallelTasks = Tasks.Select(task => Task.Run(() => taskExecuter.Execute(task, taskInputBufferMap[task], nullBuffer, executionContext))).ToArray();

            // iterate through all elements from the input buffer and copy them to they output buffer
            // also add these elements (cloned if required) to the inputBuffer of the async executing task
            foreach (var e in input.Elements)
            {
                output.Add(e);

                foreach (var b in taskInputBufferMap.Values)
                {
                    b.Add(_requiresClonedElements ? e.Clone() : e);
                }
            }

            // after iteration through input buffer is done, notify 
            foreach (var buffer in taskInputBufferMap.Values)
            {
                buffer.CompleteAdding();
            }

            // Wait for the parallel tasks to complete
            Task.WaitAll(parallelTasks);

            // dispose all created buffers
            foreach (var buffer in taskInputBufferMap.Values)
            {
                buffer.Dispose();
            }
        }

        protected virtual IBufferFactory GetBufferFactory(IExecutionContext ctx) => RequireService<IBufferFactory>(ctx);
    }
}