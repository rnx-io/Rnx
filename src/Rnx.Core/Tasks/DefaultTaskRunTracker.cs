using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Tasks
{
    public class DefaultTaskRunTracker : ITaskRunTracker
    {
        public Dictionary<string, DateTime> LastRuns { get; private set; }

        public DefaultTaskRunTracker()
        {
            this.LastRuns = new Dictionary<string, DateTime>();
        }
    }
}
