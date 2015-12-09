using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace Rnx.Common.Tasks
{
    public abstract class MultiTask : RnxTask
    {
        public ITask[] Tasks { get; private set; }
       
        public MultiTask(ITask[] tasks)
        {
            Tasks = tasks ?? new ITask[0];
        }
    }
}