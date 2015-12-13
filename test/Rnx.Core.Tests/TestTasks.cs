using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using System.Threading;
using Rnx.Abstractions.Buffers;

namespace Rnx.Core.Tests
{
    public class TestTask : RnxTask
    {
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            input.CopyTo(output);
        }
    }

    public class TestMultiTask : MultiTask
    {
        public TestMultiTask(params ITask[] tasks) : base(tasks)
        {}

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
        }
    }

    public class AsyncTestTask : IAsyncTask
    {
        public event EventHandler<AsyncTaskCompletedEventArgs> Completed;

        public string Name { get; set; }
        public string ExecutionId { get; set; }

        public void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var thread = new Thread(() =>
            {
                Thread.Sleep(500);
                Completed?.Invoke(this, new AsyncTaskCompletedEventArgs(this, executionContext, output));
            })
            { IsBackground = true };
            thread.Start();
        }
    }
}
