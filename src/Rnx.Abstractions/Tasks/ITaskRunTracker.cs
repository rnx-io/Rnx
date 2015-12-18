using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Manages the point in time of the executed user-defined tasks
    /// </summary>
    public interface ITaskRunTracker
    {
        /// <summary>
        /// Dictionary containing the task descriptor as a key and the datetime
        /// of the last run as value
        /// </summary>
        Dictionary<ITaskDescriptor,DateTime> LastTaskRuns { get; }
    }
}