using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Execution
{
    public interface IRnxTaskRunner
    {
        void Run(IEnumerable<ITask> tasks, string baseDirectory);
    }
}
