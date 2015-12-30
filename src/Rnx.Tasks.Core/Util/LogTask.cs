using Microsoft.Extensions.Logging;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Abstractions.Util;
using System;
using System.Linq;

namespace Rnx.Tasks.Core.Util
{
    public class LogTaskDescriptor : TaskDescriptorBase<LogTask>
    {
        internal Func<IBufferElement[], string> Message { get; }
        internal LogType LogType { get; private set; } = LogType.Info;

        public LogTaskDescriptor(string message)
            : this(e => message)
        { }

        public LogTaskDescriptor(Func<IBufferElement[], string> message)
        {
            Message = message;
            RequiresCompletedInputBuffer = true;
        }

        public LogTaskDescriptor AsWarning()
        {
            LogType = LogType.Warning;
            return this;
        }

        public LogTaskDescriptor AsError()
        {
            LogType = LogType.Error;
            return this;
        }
    }

    public class LogTask : RnxTask
    {
        private readonly LogTaskDescriptor _taskDescriptor;

        public LogTask(LogTaskDescriptor taskDescriptor)
        {
            _taskDescriptor = taskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var logger = LoggingContext.Current.LoggerFactory.CreateLogger(nameof(LogTask));
            Action<string> logAction = null;

            switch (_taskDescriptor.LogType)
            {
                case LogType.Info:
                    logAction = logger.LogInformation;
                    break;
                case LogType.Warning:
                    logAction = logger.LogWarning;
                    break;
                case LogType.Error:
                    logAction = logger.LogError;
                    break;
            }

            var allElements = input.Elements.ToArray();

            if (_taskDescriptor.Message != null)
            {
                logAction(_taskDescriptor.Message(allElements));
            }
        }
    }

    internal enum LogType
    {
        Info,
        Warning,
        Error
    }
}