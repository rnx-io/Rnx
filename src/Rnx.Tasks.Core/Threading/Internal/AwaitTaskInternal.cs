using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Abstractions.Buffers;

namespace Rnx.Tasks.Core.Threading.Internal
{
    public class AwaitTaskInternal : AwaitTask
    {
        private readonly IAsyncTaskManager _asyncTaskManager;

        public AwaitTaskInternal(IAsyncTaskManager asyncTaskManager)
            : base()
        {
            _asyncTaskManager = asyncTaskManager;
        }

        public AwaitTaskInternal(IAsyncTaskManager asyncTaskManager, string executionId, Action<AsyncTaskCompletedEventArgs, IBuffer, IBuffer, IExecutionContext> action = null)
            : base(executionId, action)
        {
            _asyncTaskManager = asyncTaskManager;
        }

        protected override IAsyncTaskManager GetAsyncTaskManager(IExecutionContext ctx) => _asyncTaskManager;
    }
}