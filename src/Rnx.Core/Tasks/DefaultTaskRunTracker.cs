using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Tasks
{
    /// <summary>
    /// Default implementation of <see cref="ITaskRunTracker"/>
    /// </summary>
    public class DefaultTaskRunTracker : ITaskRunTracker
    {
        public Dictionary<ITaskDescriptor, DateTime> LastTaskRuns { get; }

        public DefaultTaskRunTracker()
        {
            LastTaskRuns = new Dictionary<ITaskDescriptor, DateTime>();
        }
    }
}