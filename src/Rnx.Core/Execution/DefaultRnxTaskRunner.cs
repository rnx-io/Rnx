using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Execution
{
    /// <summary>
    /// Default implementation of <see cref="IRnxTaskRunner"/>
    /// </summary>
    public class DefaultRnxTaskRunner : IRnxTaskRunner
    {
        private ITaskExecuter _taskExecuter;
        private IServiceProvider _serviceProvider;

        public DefaultRnxTaskRunner(ITaskExecuter taskExecuter, IServiceProvider serviceProvider)
        {
            _taskExecuter = taskExecuter;
            _serviceProvider = serviceProvider;
        }

        public void Run(IEnumerable<UserDefinedTask> tasks, string baseDirectory)
        {
            if (tasks.Count() == 1)
            {
                var buffer = new NullBuffer();
                var task = tasks.First();
                _taskExecuter.Execute(task, buffer, buffer, new ExecutionContext(task.Name, baseDirectory, _serviceProvider));
            }
            else if(tasks.Count() > 1)
            {
                Parallel.ForEach(tasks, task =>
                {
                    var buffer = new NullBuffer();
                    _taskExecuter.Execute(task, buffer, buffer, new ExecutionContext(task.Name, baseDirectory, _serviceProvider));
                });
            }
        }
    }
}