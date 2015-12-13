using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;

namespace Rnx.Tasks.Core.Control.Internal
{
    public class IfTaskInternal : IfTask
    {
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public IfTaskInternal(ITaskExecuter taskExecuter, IBufferFactory bufferFactory, Predicate<IBufferElement> predicate, ITask taskToRun)
            : base(predicate, taskToRun)
        {
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        protected override IBufferFactory GetBufferFactory(IExecutionContext ctx) => _bufferFactory;
    }
}