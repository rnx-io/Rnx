using System.Collections.Generic;
using Rnx.Common.Execution;
using Rnx.Common.Buffers;

namespace Rnx.Common.Tasks
{
    public interface ITask : IExecutable
    {
        string Name { get; }
    }
}