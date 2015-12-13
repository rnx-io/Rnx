using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader
{
    /// <summary>
    /// Responsible for extracting and creating the configured tasks.
    /// </summary>
    public interface ITaskLoader
    {
        /// <summary>
        /// Returns all user-defined tasks, that match with the names in <paramref name="tasksToTun"/>
        /// </summary>
        /// <param name="taskConfigurationTypes">The types that the loader should examine</param>
        /// <param name="tasksToRun">The names of the tasks that should be run</param>
        IEnumerable<UserDefinedTask> Load(IEnumerable<Type> taskConfigurationTypes, string[] tasksToRun);
    }
}