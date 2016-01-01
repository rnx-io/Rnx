using Microsoft.Extensions.Logging;
using Microsoft.Framework.Runtime.Common.CommandLine;
using Reliak.IO.Abstractions;
using Rnx.Abstractions.Util;
using Rnx.TaskLoader;
using Rnx.Util.FileSystem;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rnx
{
    public class Program
    {
        private const string DEFAULT_TASK_NAME = "Default";
        private const string DEFAULT_RNX_FILENAME = "rnx.cs";

        public static int Main(string[] args)
        {
            var rnxApp = new CommandLineApplication { Name = "Rnx", Description = "A cross-platform C# task runner", FullName = "Rnx" };
            var taskArgument =            rnxApp.Argument("[task1 task2]", $"The name of the task(s) that should be executed. If no task is specified, Rnx looks for a '{DEFAULT_TASK_NAME}'-task", true);
            var printTasksOption =          rnxApp.Option("-t|--tasks", "Prints all available tasks", CommandOptionType.NoValue);
            var filenameOption =             rnxApp.Option("-f|--filename <FILENAME>", $"Filename to look for tasks. If omitted, Rnx will look for '{DEFAULT_RNX_FILENAME}'.", CommandOptionType.SingleValue);
            var baseDirectoryOption =       rnxApp.Option("-b|--base-directory <BASE_DIRECTORY>", "Directory path used as base directory. The default is the current working directory", CommandOptionType.SingleValue);
            var logLevelOption =            rnxApp.Option("-l|--log <LOG_LEVEL>", $"Value between 2 (most verbose) and 6 (only critical informations). The default is {CommandLineSettings.DEFAULT_LOG_LEVEL}", CommandOptionType.SingleValue);
            var silentOption =              rnxApp.Option("-s|--silent", "Disables logging to the console", CommandOptionType.NoValue);
            var versionOption =             rnxApp.Option("-v|--version", "Displays the current version", CommandOptionType.NoValue);
            var helpOption =                rnxApp.Option("-h|--help", "Displays the help", CommandOptionType.NoValue);
            var commandLineSettings = new CommandLineSettings();

            rnxApp.OnExecute(() =>
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                commandLineSettings.BaseDirectory = currentDirectory;

                if (baseDirectoryOption.HasValue())
                {
                    var userSpecifiedBaseDirectory = baseDirectoryOption.Value().TrimEnd('"');

                    if (!Path.IsPathRooted(userSpecifiedBaseDirectory))
                    {
                        userSpecifiedBaseDirectory = Path.GetFullPath(Path.Combine(currentDirectory, userSpecifiedBaseDirectory));
                    }
                    commandLineSettings.BaseDirectory = userSpecifiedBaseDirectory;
                    
                }

                var specifiedFilename = filenameOption.HasValue() ? filenameOption.Value() : DEFAULT_RNX_FILENAME;
                commandLineSettings.TaskCodeFilePath = Path.IsPathRooted(specifiedFilename) ? specifiedFilename 
                                                                : Path.GetFullPath(Path.Combine(currentDirectory, specifiedFilename));
                if (printTasksOption.HasValue())
                {
                    PrintAvailableTasks(commandLineSettings);
                    return 0;
                }

                if (versionOption.HasValue())
                {
                    var asm = typeof(Program).GetTypeInfo().Assembly;
                    var versionInfo = asm.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();

                    Console.WriteLine($"Rnx version {versionInfo.InformationalVersion}");
                    return 0;
                }

                if(helpOption.HasValue())
                {
                    rnxApp.ShowHelp();
                    return 0;
                }

                if(silentOption.HasValue())
                {
                    commandLineSettings.IsSilent = true;
                    commandLineSettings.LogLevel = (int)LogLevel.None;
                }
                else if (logLevelOption.HasValue())
                {
                    commandLineSettings.LogLevel = EnsureValidLogLevel(logLevelOption.Value());
                }

                commandLineSettings.TasksToRun = string.IsNullOrWhiteSpace(taskArgument.Value) ? new[] { DEFAULT_TASK_NAME }
                                                                                               : taskArgument.Values.ToArray();
                // Log settings
                LoggingContext.Current = new ConfigurableLoggingContext(new LoggerFactory().AddConsole((LogLevel)commandLineSettings.LogLevel));
                var logger = LoggingContext.Current.LoggerFactory.CreateLogger("Rnx");
                
                if(logger.IsEnabled(LogLevel.Verbose))
                {
                    logger.LogVerbose($"Current directory: {currentDirectory}");
                    logger.LogVerbose($"Using task file '{commandLineSettings.TaskCodeFilePath}' to determine tasks.");
                    logger.LogVerbose($"Tasks to run: {string.Join(", ", commandLineSettings.TasksToRun)}");

                    if (baseDirectoryOption.HasValue())
                    {
                        logger.LogVerbose($"Base directory specified: {commandLineSettings.BaseDirectory}");
                    }
                }

                return new RnxApp(commandLineSettings).Run();
            });

            var entryLogger = new LoggerFactory().AddConsole(LogLevel.Information).CreateLogger("Rnx");

            try
            {
                return rnxApp.Execute(args);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;

                if (ex.InnerException != null)
                {
                    errorMessage += Environment.NewLine + ex.InnerException.Message;
                }

                entryLogger.LogCritical($"Task execution error: {errorMessage}");

                if( ex is InvalidTaskNameException)
                {
                    Console.WriteLine();
                    Console.WriteLine("The following tasks are available:");
                    PrintAvailableTasks(commandLineSettings);
                    return 5;
                }
                
                return 1;
            }
            finally
            {
                if(Debugger.IsAttached)
                {
                    Console.ReadLine();
                }
            }
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

        private static void PrintAvailableTasks(CommandLineSettings commandLineSettings)
        {
            var sourceCodeResolver = new SourceCodeResolver(new DefaultFileSystem());
            var sourceCodes = sourceCodeResolver.GetCodeFileInfos(commandLineSettings.TaskCodeFilePath).Select(f => f.Content).ToArray();
            var foundTaskNames = sourceCodeResolver.FindAvailableTaskNames(sourceCodes).ToArray();

            if (foundTaskNames.Any())
            {
                foreach (var taskName in foundTaskNames)
                {
                    Console.WriteLine($"- {taskName}");
                }
            }
            else
            {
                Console.Error.WriteLine("No tasks found");
            }
        }
    }
}