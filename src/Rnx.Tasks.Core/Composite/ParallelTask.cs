﻿using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
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
            IBuffer nullBuffer = new NullBuffer();
            var taskInputBufferMap = Tasks.ToDictionary(f => f, f =>new BlockingBuffer());
            var parallelTasks = Tasks.Select(task => Task.Run(() => ExecuteTask(task, taskInputBufferMap[task], nullBuffer, executionContext))).ToArray();

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
    }
}