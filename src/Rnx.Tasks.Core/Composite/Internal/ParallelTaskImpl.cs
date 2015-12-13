using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Composite.Internal
{
    public class ParallelTaskImpl : ParallelTask
    {
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public ParallelTaskImpl(ITaskExecuter taskExecuter, IBufferFactory bufferFactory, bool requiresClonedElements, params ITask[] tasks)
            : base(requiresClonedElements, tasks)
        {
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        protected override IBufferFactory GetBufferFactory(IExecutionContext ctx) => _bufferFactory;
        protected override ITaskExecuter GetTaskExecuter(IExecutionContext ctx) => _taskExecuter;
    }
}