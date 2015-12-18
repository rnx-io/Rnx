using Rnx.Abstractions.Execution.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Rnx.Core.Execution.Decorators
{
    /// <summary>
    /// Default logging decorator for all tasks
    /// </summary>
    public class DefaultLoggingTaskDecorator : AbstractLoggingTaskDecorator
    {
        private readonly ILoggerFactory _loggerFactory;

        public DefaultLoggingTaskDecorator(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public override void Execute(IDecoratorQueue decoratorQueue, ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var taskName = task.Name;
            var logger = _loggerFactory.CreateLogger(taskName);
            logger.LogInformation("Starting task {0}...", taskName);

            var stopwatch = Stopwatch.StartNew();

            decoratorQueue.GetNext()?.Execute(decoratorQueue, task, input, output, executionContext);

            stopwatch.Stop();
            logger.LogInformation("Task {0} completed in {1} ms", taskName, stopwatch.ElapsedMilliseconds);
        }
    }
}