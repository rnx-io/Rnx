using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Threading
{
    public class AsyncTaskDescriptor : TaskDescriptorBase<AsyncTask>
    {
        public ITaskDescriptor TaskDescriptorToRunAsynchronously { get; }
        public string ExecutionId { get; }

        public AsyncTaskDescriptor(ITaskDescriptor taskDescriptorToRunAsynchronously, string executionId = null)
        {
            ExecutionId = executionId ?? Guid.NewGuid().ToString();
            TaskDescriptorToRunAsynchronously = taskDescriptorToRunAsynchronously;
        }
    }

    public class AsyncTask : RnxTask, IAsyncTask
    {
        public event EventHandler<AsyncTaskCompletedEventArgs> Completed;

        private readonly AsyncTaskDescriptor _asyncTaskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public AsyncTask(AsyncTaskDescriptor asyncTaskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _asyncTaskDescriptor = asyncTaskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        public string ExecutionId => _asyncTaskDescriptor.ExecutionId;

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            IBuffer taskToRunInputBuffer = _bufferFactory.Create();

            Task.Run(() =>
            {
                try
                {
                    var outputBuffer = _bufferFactory.Create();
                    _taskExecuter.Execute(_asyncTaskDescriptor.TaskDescriptorToRunAsynchronously, taskToRunInputBuffer, outputBuffer, executionContext);
                    Completed?.Invoke(this, new AsyncTaskCompletedEventArgs(this, executionContext, outputBuffer));
                }
                catch (Exception ex)
                {
                    Completed?.Invoke(this, new AsyncTaskCompletedEventArgs(this, executionContext, ex.Message));
                }
            });

            // iterate through all elements from the input buffer and copy them to they output buffer
            // also add the cloned elements to the inputBuffer of the async executing task
            foreach (var e in input.Elements)
            {
                output.Add(e);
                taskToRunInputBuffer.Add(e.Clone());
            }

            // signal async task, that all elements were added
            taskToRunInputBuffer.CompleteAdding();
        }
    }
}