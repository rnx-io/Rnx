using Rnx.Common.Buffers;
using Rnx.Common.Configuration;
using Rnx.Common.Exceptions;
using Rnx.Common.Execution;
using Rnx.Common.Execution.Decorators;
using Rnx.Common.Tasks;
using Rnx.Common.Util;
using Rnx.Core.Buffers;
using Rnx.Core.Configuration;
using Rnx.Core.Execution;
using Rnx.Core.Execution.Decorators;
using Rnx.Core.Tasks;
using Rnx.Core.Util;
using Microsoft.Dnx.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Framework.Runtime.Common.CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Dnx.Runtime;

namespace Rnx
{
    public class Program
    {
        private const string DEFAULT_TASK_NAME = "Default";
        
        public static int Main(string[] args)
        {
            Console.WriteLine("Starting Rnx...");

            var stopwatch = Stopwatch.StartNew();

            var rnxApp = new CommandLineApplication { Name = "Rnx", Description = "A cross-platform C# task runner" };
            var taskArgument =              rnxApp.Argument("[task1 task2]", "The name of the task(s) that should be executed", true);
            var rnxProjectDirectoryOption = rnxApp.Option("-d|--rnx-project-dir <RNX_PROJECT_DIR>", "Specifies the directory for the Rnx configuration file", CommandOptionType.SingleValue);
            var baseDirectoryOption =       rnxApp.Option("-b|--base-directory <BASE_DIRECTORY>", "Directory path used as base directory", CommandOptionType.SingleValue);
            var silentOption =              rnxApp.Option("-s|--silent", "Disables logging to the console", CommandOptionType.NoValue);
            var logLevelOption =            rnxApp.Option("-l|--log <LOG_LEVEL>", $"Value between 1 (most verbose) and 6 (only critical informations). The default is {CommandLineSettings.DEFAULT_LOG_LEVEL}", CommandOptionType.SingleValue);

            rnxApp.OnExecute(() =>
            {
                var commandLineSettings = new CommandLineSettings();
                commandLineSettings.IsSilent = silentOption.HasValue();
                commandLineSettings.TasksToRun = string.IsNullOrWhiteSpace(taskArgument.Value) ? new[] { DEFAULT_TASK_NAME }
                                                                                               : taskArgument.Values.ToArray();
                if( logLevelOption.HasValue())
                {
                    commandLineSettings.LogLevel = EnsureValidLogLevel(logLevelOption.Value());
                }

                string currentDirectory = Directory.GetCurrentDirectory();
                string rnxProjectDirectory = PlatformServices.Default.Application.ApplicationBasePath;

                if (rnxProjectDirectoryOption.HasValue())
                {
                    rnxProjectDirectory = Path.GetFullPath(Path.Combine(currentDirectory, rnxProjectDirectoryOption.Value()));

                    if (!Directory.Exists(rnxProjectDirectory))
                    {
                        return LogFailed($"Can not find Rnx project directory '{rnxProjectDirectory}'. Please specify a valid directory.");
                    }
                }

                // we use the current directory as base directory, except when a base directory is provided via command line
                commandLineSettings.BaseDirectory = baseDirectoryOption.HasValue() ? baseDirectoryOption.Value() : currentDirectory;
                commandLineSettings.RnxProjectDirectory = rnxProjectDirectory;

                var result = Run(commandLineSettings);
                return result;
            });

            try
            {
                return rnxApp.Execute(args);
            }
            catch (Exception ex)
            {
                var errorCode = LogFailed($"Task execution error: {ex.Message}");
                return errorCode;
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine("Total: " + stopwatch.ElapsedMilliseconds + " ms");
            }
        }
                
        private static int Run(CommandLineSettings commandLineSettings)
        {
            // Setup
            var serviceProvider = ConfigureServices(commandLineSettings);
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Rnx");

            try
            {
                // Load tasks
                var configLoader = serviceProvider.GetService<ITaskConfigurationTypeLoader>();
                var configTypes = configLoader.Load(commandLineSettings.RnxProjectDirectory);
                var taskLoader = serviceProvider.GetService<ITaskLoader>();
                var tasksToRun = taskLoader.Load(configTypes, commandLineSettings.TasksToRun).ToArray();
                var asyncTaskManager = serviceProvider.GetService<IAsyncTaskManager>();

                // Validate user input from command line
                if (tasksToRun.Length != commandLineSettings.TasksToRun.Length)
                {
                    var invalidTaskNames = commandLineSettings.TasksToRun.Except(tasksToRun.Select(f => f.Name), StringComparer.OrdinalIgnoreCase).ToArray();
                    throw new RnxException($"Invalid task name(s): {string.Join(", ", invalidTaskNames)}");
                }

                // Run
                var taskRunner = serviceProvider.GetService<IRnxTaskRunner>();
                taskRunner.Run(tasksToRun, commandLineSettings.BaseDirectory);

                if (asyncTaskManager.HasUncompletedTasks)
                {
                    logger.LogInformation("Synchronous tasks completed. Waiting for asynchronous tasks to complete...");
                    asyncTaskManager.WaitAll();
                }

                logger.LogInformation("All tasks completed.");

                return 0;
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message, ex);
                throw ex;
            }
        }

        private static IServiceProvider ConfigureServices(CommandLineSettings commandLineSettings)
        {
            ILibraryExporter libExporter = CallContextServiceLocator.Locator.ServiceProvider.GetService<ILibraryExporter>();
            IBootstrapper bootstrapper = new DefaultBootstrapper(PlatformServices.Default.Application, libExporter, PlatformServices.Default.AssemblyLoadContextAccessor);
            bootstrapper.Initialize();

            ILoggerFactory loggerFactory;

            if (commandLineSettings.IsSilent)
            {
                loggerFactory = new NullLoggerFactory();
            }
            else
            {
                loggerFactory = new LoggerFactory().AddConsole((LogLevel)commandLineSettings.LogLevel);
            }

            var services = new ServiceCollection();
            services.AddInstance(typeof(IBootstrapper), bootstrapper)
                    .AddInstance(typeof(IMetaDataReferenceProvider), bootstrapper)
                    .AddInstance(typeof(ILoggerFactory), loggerFactory);

            services.AddSingleton<ICodeCompiler, DefaultCodeCompiler>()
                    .AddSingleton<ITaskLoader, DefaultTaskLoader>()
                    .AddSingleton<ICallingProjectLocator>(f => new DefaultCallingProjectLocator(commandLineSettings.BaseDirectory))
                    .AddSingleton<ITaskConfigurationTypeLoader, DefaultTaskConfigurationTypeLoader>()
                    .AddSingleton<IBufferElementFactory, DefaultBufferElementFactory>()
                    .AddSingleton<IAsyncTaskManager, DefaultAsyncTaskManager>()
                    .AddSingleton<ITaskRunTracker, DefaultTaskRunTracker>()
                    .AddSingleton<DefaultTaskExecuter, DefaultTaskExecuter>()
                    .AddSingleton<ITaskDecoratorService, DefaultTaskDecoratorService>()
                    .AddSingleton<ITaskExecutionDecorator, AsyncTaskExecutionDecorator>()
                    .AddSingleton<ITaskExecutionDecorator, BufferTaskExecutionDecorator>()
                    .AddSingleton<ITaskExecutionDecorator, DefaultLoggingExecutionDecorator>()
                    .AddSingleton<ITaskExecutionDecorator, LastRunTaskExecutionDecorator>()
                    .AddSingleton<ITaskExecuter, DefaultTaskExecuter>()
                    .AddSingleton<IRnxTaskRunner>(f =>
                    {
                        return new DefaultRnxTaskRunner(f.GetService<ITaskExecuter>(), f.GetService<ILoggerFactory>(), f);
                    });

            // add existing framework services
            foreach (var service in GetFrameworkServices())
            {
                services.AddInstance(service.Item1, service.Item2);
            }

            var serviceRegistrations = bootstrapper.GetServiceRegistrations();

            foreach (var serviceRegistration in serviceRegistrations)
            {
                serviceRegistration.RegisterServices(services);
            }

            var serviceProvider = services.BuildServiceProvider();

            foreach (var serviceRegistration in serviceRegistrations)
            {
                serviceRegistration.ConfigureServices(serviceProvider);
            }

            return serviceProvider;
        }

        private static IEnumerable<Tuple<Type,object>> GetFrameworkServices()
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

        private static int LogFailed(string message)
        {
            Console.WriteLine(message);
            return 2;
        }

        private static int EnsureValidLogLevel(string input)
        {
            int result;

            if( !int.TryParse(input, out result ) )
            {
                return CommandLineSettings.DEFAULT_LOG_LEVEL;
            }

            int min = (int)LogLevel.Debug;
            int max = (int)LogLevel.Critical;
            return result < min ? min : (result > max ? max : result);
        }
    }
}