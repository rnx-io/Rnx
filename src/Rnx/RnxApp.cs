using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Exceptions;
using Microsoft.Extensions.PlatformAbstractions;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Core.Execution;
using Microsoft.Dnx.Compilation;
using Rnx.Abstractions.Util;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Tasks;
using Rnx.Core.Tasks;
using Rnx.Abstractions.Execution;
using Rnx.Core.Buffers;
using Rnx.Core.Execution.Decorators;
using System.IO;
using Rnx.TaskLoader;
using Rnx.TaskLoader.Compilation;
using Reliak.IO.Abstractions;
using Rnx.Util.FileSystem;

namespace Rnx
{
    public class RnxApp
    {
        private readonly CommandLineSettings _commandLineSettings;
        private readonly ILoggerFactory _loggerFactory;

        public RnxApp(CommandLineSettings commandLineSettings, ILoggerFactory loggerFactory)
        {
            _commandLineSettings = commandLineSettings;
            _loggerFactory = loggerFactory;
        }

        public int Run()
        {
            // Setup
            var serviceProvider = ConfigureServices();
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Rnx");

            // Load tasks
            var rnxProjectLoader = serviceProvider.GetService<IRnxProjectLoader>();
            var configTypes = rnxProjectLoader.Load(_commandLineSettings.RnxProjectDirectory);
            var taskLoader = serviceProvider.GetService<ITaskLoader>();
            var tasksToRun = taskLoader.Load(configTypes, _commandLineSettings.TasksToRun).ToArray();

            // Validate user input from command line
            if (tasksToRun.Length != _commandLineSettings.TasksToRun.Length)
            {
                var invalidTaskNames = _commandLineSettings.TasksToRun.Except(tasksToRun.Select(f => f.Name), StringComparer.OrdinalIgnoreCase).ToArray();
                throw new RnxException($"Invalid task name(s): {string.Join(", ", invalidTaskNames)}");
            }

            if (!tasksToRun.Any())
            {
                logger.LogWarning("No tasks specified.");
                return 0;
            }

            // Run
            var taskRunner = serviceProvider.GetService<IRnxTaskRunner>();
            taskRunner.Run(tasksToRun, _commandLineSettings.BaseDirectory);

            var asyncTaskManager = serviceProvider.GetService<IAsyncTaskManager>();

            if (asyncTaskManager.HasUncompletedTasks)
            {
                logger.LogInformation("Synchronous tasks completed. Waiting for asynchronous tasks to complete...");
                asyncTaskManager.WaitAll();
            }

            logger.LogInformation("All tasks completed.");

            return 0;
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddInstance(typeof(ILoggerFactory), _loggerFactory)
                    .AddSingleton<ICodeCompiler, DefaultCodeCompiler>()
                    .AddSingleton<IMetaDataReferenceProvider, DefaultMetaDataReferenceProvider>()
                    .AddSingleton<ITaskLoader, DefaultTaskLoader>()
                    .AddSingleton<IRnxProjectLoader, DefaultRnxProjectLoader>()
                    .AddSingleton<IBufferElementFactory, DefaultBufferElementFactory>()
                    .AddSingleton<IAsyncTaskManager, DefaultAsyncTaskManager>()
                    .AddSingleton<ITaskRunTracker, DefaultTaskRunTracker>()
                    .AddSingleton<ITaskDecorator, AsyncTaskDecorator>()
                    .AddSingleton<ITaskDecorator, BufferTaskDecorator>()
                    .AddSingleton<ITaskDecorator, DefaultLoggingTaskDecorator>()
                    .AddSingleton<ITaskDecorator, LastRunTaskDecorator>()
                    .AddSingleton<IBufferFactory, BlockingBufferFactory>()
                    .AddSingleton<IFileSystem, DefaultFileSystem>()
                    .AddSingleton<IGlobMatcher, DefaultGlobMatcher>()
                    .AddSingleton<ITaskExecuter>(f =>
                    {
                        return new DefaultTaskExecuter(f.GetServices<ITaskDecorator>());
                    })
                    .AddSingleton<IRnxTaskRunner>(f =>
                    {
                        return new DefaultRnxTaskRunner(f.GetService<ITaskExecuter>(), f);
                    });

            // add existing framework services
            foreach (var service in GetFrameworkServices())
            {
                services.AddInstance(service.Item1, service.Item2);
            }

            return services.BuildServiceProvider();
        }

        private IEnumerable<Tuple<Type, object>> GetFrameworkServices()
        {
            var frameworkServiceProvider = CallContextServiceLocator.Locator.ServiceProvider;
            var runtimeServices = frameworkServiceProvider.GetService<IRuntimeServices>();

            if (runtimeServices != null)
            {
                foreach (var serviceType in runtimeServices.Services)
                {
                    yield return new Tuple<Type, object>(serviceType, frameworkServiceProvider.GetService(serviceType));
                }
            }
        }
    }
}