using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Core.Util;

namespace Rnx.Core.Execution
{
    /// <summary>
    /// Default implementation of <see cref="IRnxTaskRunner"/>
    /// </summary>
    public class DefaultRnxTaskRunner : IRnxTaskRunner
    {
        private ITaskExecuter _taskExecuter;

        public DefaultRnxTaskRunner(ITaskExecuter taskExecuter)
        {
            _taskExecuter = taskExecuter;
        }

        public void Run(IEnumerable<ITaskDescriptor> tasks, string baseDirectory)
        {
            if (tasks.Count() == 1)
            {
                var buffer = new NullBuffer();
                var task = tasks.First();
                _taskExecuter.Execute(task, buffer, buffer, new ExecutionContext(task, baseDirectory));
            }
            else if(tasks.Count() > 1)
            {
                Parallel.ForEach(tasks, task =>
                {
                    var buffer = new NullBuffer();
                    _taskExecuter.Execute(task, buffer, buffer, new ExecutionContext(task, baseDirectory));
                });
            }
        }
    }
}