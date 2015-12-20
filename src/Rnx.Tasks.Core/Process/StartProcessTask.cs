using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Rnx.Tasks.Core.Process
{
    public class StartProcessTaskDescriptor : TaskDescriptorBase<StartProcessTask>
    {
        internal ProcessStartInfo ProcessStartInfo { get; private set; }
        internal bool WaitForExit { get; private set; }

        public StartProcessTaskDescriptor(string filename, string arguments = "", bool waitForExit = true, bool redirectOutput = true)
            : this(new ProcessStartInfo(filename, arguments)
            {
                CreateNoWindow = true, RedirectStandardOutput = redirectOutput, RedirectStandardError = redirectOutput
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
        private readonly ILoggerFactory _loggerFactory;

        public StartProcessTask(StartProcessTaskDescriptor taskDescriptor, ILoggerFactory loggerFactory)
        {
            _taskDescriptor = taskDescriptor;
            _loggerFactory = loggerFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var logger = _loggerFactory.CreateLogger("StartProcess");
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