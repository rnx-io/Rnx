
using Rnx.Common.Buffers;
using Rnx.Common.Tasks;
using System.Collections.Generic;

namespace Rnx.Common.Execution
{
    public interface ITaskExecuter
    {
        void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}
