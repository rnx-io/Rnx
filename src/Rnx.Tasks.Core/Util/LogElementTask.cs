using Microsoft.Extensions.Logging;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Abstractions.Util;
using System;

namespace Rnx.Tasks.Core.Util
{
    public class LogElementTaskDescriptor : TaskDescriptorBase<LogElementTask>
    {
        internal Func<IBufferElement, string> Message { get; }

        public LogElementTaskDescriptor(Func<IBufferElement, string> message)
        {
            Message = message;
        }
    }

    public class LogElementTask : RnxTask
    {
        private readonly LogElementTaskDescriptor _taskDescriptor;

        public LogElementTask(LogElementTaskDescriptor taskDescriptor)
        {
            _taskDescriptor = taskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var logger = LoggingContext.Current.LoggerFactory.CreateLogger(nameof(LogTask));

            foreach (var e in input.Elements)
            {
                logger.LogInformation(_taskDescriptor.Message(e));
                output.Add(e);
            }
        }
    }
}