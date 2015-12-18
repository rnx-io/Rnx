using System;

namespace Rnx.Abstractions.Execution.Decorators
{
    /// <summary>
    /// Base class for all task-specific decorators that want to override the logging decorator for a specific task
    /// </summary>
    public abstract class AbstractLoggingTaskDecorator : AbstractTaskDecorator, ILoggingTaskDecorator
    {
        public override Type ExportType => typeof(ILoggingTaskDecorator);
    }
}