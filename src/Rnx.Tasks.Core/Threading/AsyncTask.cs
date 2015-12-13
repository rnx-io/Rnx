using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Threading
{
    public class AsyncTask : RnxTask, IAsyncTask
    {
        public event EventHandler<AsyncTaskCompletedEventArgs> Completed;

        private ITask _taskToRunAsynchronously;
        private bool _requiresClonedElements;

        public string ExecutionId { get; private set; }

        public AsyncTask(ITask taskToRunAsynchronously, string executionId = null, bool requiresClonedElements = false)
        {
            ExecutionId = executionId ?? Guid.NewGuid().ToString();
            _taskToRunAsynchronously = taskToRunAsynchronously;
            _requiresClonedElements = requiresClonedElements;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferFactory = GetBufferFactory(executionContext);
            var taskExecuter = GetTaskExecuter(executionContext);
            IBuffer taskToRunInputBuffer = bufferFactory.Create();

            Task.Run(() =>
            {
                try
                {
                    var outputBuffer = bufferFactory.Create();
                    taskExecuter.Execute(_taskToRunAsynchronously, taskToRunInputBuffer, outputBuffer, executionContext);
                    Completed?.Invoke(this, new AsyncTaskCompletedEventArgs(this, executionContext, outputBuffer));
                }
                catch (Exception ex)
                {
                    Completed?.Invoke(this, new AsyncTaskCompletedEventArgs(this, executionContext, ex.Message));
                }
            });

            // iterate through all elements from the input buffer and copy them to they output buffer
            // also add these elements (cloned if required) to the inputBuffer of the async executing task
            foreach (var e in input.Elements)
            {
                output.Add(e);
                taskToRunInputBuffer.Add(_requiresClonedElements ? e.Clone() : e);
            }

            // signal running task, that all elements were added
            taskToRunInputBuffer.CompleteAdding();
        }

        protected virtual IBufferFactory GetBufferFactory(IExecutionContext ctx) => RequireService<IBufferFactory>(ctx);
        
    }
}