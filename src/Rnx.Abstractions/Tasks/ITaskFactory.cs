using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// A task factory is responsible for creating a task from a specified task descriptor
    /// </summary>
    public interface ITaskFactory
    {
        /// <summary>
        /// Creates a task from a task descriptor
        /// </summary>
        ITask Create(ITaskDescriptor taskDescriptor);
    }
}