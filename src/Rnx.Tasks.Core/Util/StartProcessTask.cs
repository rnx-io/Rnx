using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Rnx.Abstractions.Util;

namespace Rnx.Tasks.Core.Util
{
    public class StartProcessTaskDescriptor : TaskDescriptorBase<StartProcessTask>
    {
        internal ProcessStartInfo ProcessStartInfo { get; private set; }
        internal bool WaitForExit { get; private set; }

        public StartProcessTaskDescriptor(string filename, string arguments = "", bool waitForExit = true, bool redirectOutput = true)
            : this(new ProcessStartInfo(filename, arguments)
            {
                CreateNoWindow = true, RedirectStandardOutput = redirectOutput, RedirectStandardError = redirectOutput, UseShellExecute = false
            }, waitForExit)
        { }

        public StartProcessTaskDescriptor(ProcessStartInfo processStartInfo, bool waitForExit = true)
        {
            ProcessStartInfo = processStartInfo;
            WaitForExit = waitForExit;
        }
    }

    public class StartProcessTask : RnxTask
    {
        private readonly StartProcessTaskDescriptor _taskDescriptor;

        public StartProcessTask(StartProcessTaskDescriptor taskDescriptor)
        {
            _taskDescriptor = taskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var logger = LoggingContext.Current.LoggerFactory.CreateLogger("StartProcess");
            var process = new System.Diagnostics.Process();
            process.StartInfo = _taskDescriptor.ProcessStartInfo;

            if(_taskDescriptor.ProcessStartInfo.RedirectStandardOutput)
            {
                process.OutputDataReceived += (s, e) => { if (e.Data != null) { logger.LogVerbose(e.Data); } };
            }

            if (_taskDescriptor.ProcessStartInfo.RedirectStandardError)
            {
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) { logger.LogError(e.Data); } };
            }

            process.Start();

            if (_taskDescriptor.ProcessStartInfo.RedirectStandardOutput)
            {
                process.BeginOutputReadLine();
            }

            if (_taskDescriptor.ProcessStartInfo.RedirectStandardError)
            {
                process.BeginErrorReadLine();
            }

            if (_taskDescriptor.WaitForExit)
            {
                process.WaitForExit();
            }
        }
    }
}