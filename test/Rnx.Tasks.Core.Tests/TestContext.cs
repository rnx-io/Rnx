using Rnx.Common.Execution;
using Rnx.Common.Util;
using Rnx.Core.Execution;
using Rnx.Core.Util;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Tasks.Core.FileSystem;
using Microsoft.Extensions.Logging;
using Rnx.Common.Buffers;
using Rnx.Common.Tasks;
using Rnx.Core.Buffers;

namespace Rnx.Tasks.Core.Tests
{
    public class TestContext
    {
        public ExecutionContext ExecutionContext { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public string SampleDirectory { get; private set; }

        public TestContext(string sampleFolder)
        {
            var sampleRoot = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Samples");
            SampleDirectory = Path.Combine(sampleRoot, sampleFolder);
            var configFilename = Path.Combine(SampleDirectory, "RnxConfig.cs");

            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IFileSystem, DefaultFileSystem>();
            services.AddSingleton<IBufferElementFactory, DefaultBufferElementFactory>();
            services.AddSingleton<ILoggerFactory,LoggerFactory>();
            services.AddSingleton<ITaskExecuter, TestTaskExecuter>();

            ServiceProvider = services.BuildServiceProvider();
            ExecutionContext = new ExecutionContext("Test", Path.GetDirectoryName(configFilename), ServiceProvider);
        }

        private class TestTaskExecuter : ITaskExecuter
        {
            public void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                task.Execute(input, output, executionContext);
                output?.CompleteAdding();
            }
        }
    }
}
