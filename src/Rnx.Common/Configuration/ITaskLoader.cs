using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Configuration
{
    public interface ITaskLoader
    {
        IEnumerable<ITask> Load(IEnumerable<Type> taskConfigurationTypes, string[] tasksToRun);
    }
}
