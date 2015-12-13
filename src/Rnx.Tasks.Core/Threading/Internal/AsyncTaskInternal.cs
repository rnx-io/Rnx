using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Threading.Internal
{
    public class AsyncTaskInternal : AsyncTask
    {
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public AsyncTaskInternal(ITaskExecuter taskExecuter, IBufferFactory bufferFactory, ITask taskToRunAsynchronously, string executionId = null, bool requiresClonedElements = false)
            : base(taskToRunAsynchronously, executionId, requiresClonedElements)
        {
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        protected override ITaskExecuter GetTaskExecuter(IExecutionContext ctx) => _taskExecuter;
        protected override IBufferFactory GetBufferFactory(IExecutionContext ctx) => _bufferFactory;
    }
}