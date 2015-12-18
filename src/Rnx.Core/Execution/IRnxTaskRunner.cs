using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Execution
{
    /// <summary>
    /// Responsible for running the user defined tasks
    /// </summary>
    public interface IRnxTaskRunner
    {
        /// <summary>
        /// Runs the user defined tasks
        /// </summary>
        /// <param name="tasks">The user-defined tasks that should be run</param>
        /// <param name="baseDirectory">The base directory that will be used by all tasks as the "current directory"</param>
        void Run(IEnumerable<ITaskDescriptor> tasks, string baseDirectory);
    }
}