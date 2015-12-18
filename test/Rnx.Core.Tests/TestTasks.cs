using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using System.Threading;
using Rnx.Abstractions.Buffers;
using System.Threading.Tasks;

namespace Rnx.Core.Tests
{
    public class AsyncTestTask : IAsyncTask
    {
        public event EventHandler<AsyncTaskCompletedEventArgs> Completed;

        public string Name { get; set; }
        public string ExecutionId { get; set; }

        public void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);
                Completed?.Invoke(this, new AsyncTaskCompletedEventArgs(this, executionContext, output));
            });
        }
    }
}
