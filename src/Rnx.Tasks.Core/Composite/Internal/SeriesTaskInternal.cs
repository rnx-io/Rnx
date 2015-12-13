using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Composite.Internal
{
    public class SeriesTaskInternal : SeriesTask
    {
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public SeriesTaskInternal(ITaskExecuter taskExecuter, IBufferFactory bufferFactory, params ITask[] tasks)
            : base(tasks)
        {
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        protected override IBufferFactory GetBufferFactory(IExecutionContext ctx) => _bufferFactory;
        protected override ITaskExecuter GetTaskExecuter(IExecutionContext ctx) => _taskExecuter;
    }
}