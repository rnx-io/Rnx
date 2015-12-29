using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Util;
using Microsoft.Extensions.Logging;

namespace Rnx.Tasks.Core.Util
{
    public class LogTaskDescriptor : TaskDescriptorBase<LogTask>
    {
        internal string Message { get; }
        internal LogType LogType { get; private set; } = LogType.Info;

        public LogTaskDescriptor(string message)
        {
            Message = message;
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

            switch (_taskDescriptor.LogType)
            {
                case LogType.Info:
                    logger.LogInformation(_taskDescriptor.Message);
                    break;
                case LogType.Warning:
                    logger.LogWarning(_taskDescriptor.Message);
                    break;
                case LogType.Error:
                    logger.LogError(_taskDescriptor.Message);
                    break;
                default:
                    break;
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