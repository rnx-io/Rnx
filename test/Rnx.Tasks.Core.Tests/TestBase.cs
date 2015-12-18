using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Core.Buffers;
using Rnx.Core.Execution;
using Rnx.Core.Tasks;
using Rnx.Tasks.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Tests
{
    public class TestBase
    {
        private IServiceCollection _services;
        private Lazy<IServiceProvider> _lazyServiceProvider;

        public TestBase()
        {
            _services = new ServiceCollection();
            _lazyServiceProvider = new Lazy<IServiceProvider>(() => _services.BuildServiceProvider());

            AddSingleton<ITaskExecuter, DefaultTaskExecuter>();
            AddSingleton<ITaskFactory, DefaultTaskFactory>();
            AddSingleton<IBufferFactory, BlockingBufferFactory>();
            AddSingleton<IBufferElementFactory, DefaultBufferElementFactory>();
        }

        protected TestBase AddSingleton<TService,TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            _services.AddSingleton<TService, TImplementation>();
            return this;
        }

        protected IBuffer GenerateContentBuffer(int count)
        {
            var buffer = _lazyServiceProvider.Value.GetService<IBufferFactory>().Create();
            ExecuteTask(new GenerateContentTaskDescriptor(count), new NullBuffer(), buffer, null);
            return buffer;
        }

        private ITaskExecuter TaskExecuter => _lazyServiceProvider.Value.GetService<ITaskExecuter>();

        protected void ExecuteTask(ITaskDescriptor taskDescriptor, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            TaskExecuter.Execute(taskDescriptor, input, output, executionContext);
        }
    }
}