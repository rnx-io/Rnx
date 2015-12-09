using Rnx.Common.Execution.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Rnx.Core.Execution.Decorators
{
    public class DefaultLoggingExecutionDecorator : AbstractLoggingExecutionDecorator
    {
        public override void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var taskName = task.Name;
            var logger = executionContext.ServiceProvider.GetService<ILoggerFactory>().CreateLogger(taskName);
            logger.LogInformation("Starting task {0}...", taskName);

            var stopwatch = Stopwatch.StartNew();

            Decoratee.Execute(task, input, output, executionContext);

            stopwatch.Stop();
            logger.LogInformation("Task {0} completed in {1} ms", taskName, stopwatch.ElapsedMilliseconds);
        }
    }
}