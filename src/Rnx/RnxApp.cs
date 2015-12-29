using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Reliak.IO.Abstractions;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Exceptions;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using Rnx.Abstractions.Util;
using Rnx.Core.Buffers;
using Rnx.Core.Execution;
using Rnx.Core.Execution.Decorators;
using Rnx.Core.Tasks;
using Rnx.TaskLoader;
using Rnx.TaskLoader.Compilation;
using Rnx.Util.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnx
{
    public class RnxApp
    {
        private readonly CommandLineSettings _commandLineSettings;

        public RnxApp(CommandLineSettings commandLineSettings)
        {
            _commandLineSettings = commandLineSettings;
        }

        public int Run()
        {
            // Setup
            var serviceProvider = ConfigureServices();
            var logger = LoggingContext.Current.LoggerFactory.CreateLogger("Rnx");

            // Load tasks
            var rnxProjectLoader = serviceProvider.GetService<ITaskTypeLoader>();
            var configTypes = rnxProjectLoader.Load(_commandLineSettings.BaseDirectory, _commandLineSettings.TaskSourceCodeGlobPaths);
            var taskLoader = serviceProvider.GetService<ITaskLoader>();
            var tasksToRun = taskLoader.Load(configTypes, _commandLineSettings.TasksToRun).ToArray();

            // Validate user input from command line
            if (tasksToRun.Length != _commandLineSettings.TasksToRun.Length)
            {
                var invalidTaskNames = _commandLineSettings.TasksToRun.Except(tasksToRun.Select(f => f.UserDefinedTaskName), StringComparer.OrdinalIgnoreCase).ToArray();
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
            services.AddSingleton<LastRunTaskDecorator,LastRunTaskDecorator>()
                    .AddSingleton<ICodeCompiler, DefaultCodeCompiler>()
                    .AddSingleton<IMetaDataReferenceProvider, DefaultMetaDataReferenceProvider>()
                    .AddSingleton<ITaskLoader, DefaultTaskLoader>()
                    .AddSingleton<ITaskTypeLoader, DefaultTaskTypeLoader>()
                    .AddSingleton<IBufferElementFactory, DefaultBufferElementFactory>()
                    .AddSingleton<IAsyncTaskManager, DefaultAsyncTaskManager>()
                    .AddSingleton<ITaskRunTracker, DefaultTaskRunTracker>()
                    .AddSingleton<ITaskFactory, DefaultTaskFactory>()
                    .AddSingleton<ITaskDecorator, AsyncTaskDecorator>()
                    .AddSingleton<ITaskDecorator, DefaultLoggingTaskDecorator>()
                    .AddSingleton<IBufferFactory, BlockingBufferFactory>()
                    .AddSingleton<IFileSystem, DefaultFileSystem>()
                    .AddSingleton<IGlobMatcher, DefaultGlobMatcher>()
                    .AddSingleton<IRnxTaskRunner, DefaultRnxTaskRunner>()
                    .AddSingleton<ITaskExecuter>(f =>
                    {
                        return new DefaultTaskExecuter(f.GetService<ITaskFactory>(), f.GetServices<ITaskDecorator>());
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

            foreach (var serviceType in runtimeServices.Services)
            {
                yield return new Tuple<Type, object>(serviceType, frameworkServiceProvider.GetService(serviceType));
            }
        }
    }
}