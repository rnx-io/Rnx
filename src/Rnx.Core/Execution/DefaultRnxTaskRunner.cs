using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Execution
{
    public class DefaultRnxTaskRunner : IRnxTaskRunner
    {
        private ITaskExecuter _taskExecuter;
        private ILoggerFactory _loggerFactory;
        private IServiceProvider _serviceProvider;

        public DefaultRnxTaskRunner(ITaskExecuter taskExecuter, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            _taskExecuter = taskExecuter;
            _loggerFactory = loggerFactory;
            _serviceProvider = serviceProvider;
        }

        public void Run(IEnumerable<ITask> tasks, string baseDirectory)
        {
            var logger = _loggerFactory.CreateLogger("Rnx");

            if (!tasks.Any())
            {
                logger.LogWarning("No tasks specified.");
                return;
            }

            if (tasks.Count() == 1)
            {
                var buffer = new NullBuffer();
                var task = tasks.First();
                _taskExecuter.Execute(task, buffer, buffer, new ExecutionContext(task.Name, baseDirectory, _serviceProvider));
            }
            else
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