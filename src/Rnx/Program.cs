using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Framework.Runtime.Common.CommandLine;
using System;
using System.Diagnostics;
using System.IO;

namespace Rnx
{
    public class Program
    {
        private const string DEFAULT_TASK_NAME = "Default";

        public static int Main(string[] args)
        {
            var entryLogger = new LoggerFactory().AddConsole(LogLevel.Information).CreateLogger("Rnx");
            entryLogger.LogInformation("Starting Rnx...");

            var stopwatch = Stopwatch.StartNew();
            var rnxApp = new CommandLineApplication { Name = "Rnx", Description = "A cross-platform C# task runner" };
            var taskArgument =              rnxApp.Argument("[task1 task2]", "The name of the task(s) that should be executed", true);
            var rnxProjectDirectoryOption = rnxApp.Option("-p|--rnx-project-dir <RNX_PROJECT_DIR>", "Specifies the directory for the Rnx configuration file", CommandOptionType.SingleValue);
            var baseDirectoryOption =       rnxApp.Option("-b|--base-directory <BASE_DIRECTORY>", "Directory path used as base directory", CommandOptionType.SingleValue);
            var silentOption =              rnxApp.Option("-s|--silent", "Disables logging to the console", CommandOptionType.NoValue);
            var logLevelOption =            rnxApp.Option("-l|--log <LOG_LEVEL>", $"Value between 2 (most verbose) and 6 (only critical informations). The default is {CommandLineSettings.DEFAULT_LOG_LEVEL}", CommandOptionType.SingleValue);

            rnxApp.OnExecute(() =>
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                var commandLineSettings = new CommandLineSettings();
                commandLineSettings.BaseDirectory = currentDirectory;

                if(silentOption.HasValue())
                {
                    commandLineSettings.IsSilent = true;
                    commandLineSettings.LogLevel = (int)LogLevel.None;
                }
                else if (logLevelOption.HasValue())
                {
                    commandLineSettings.LogLevel = EnsureValidLogLevel(logLevelOption.Value());
                }

                var loggerFactory = new LoggerFactory().AddConsole((LogLevel)commandLineSettings.LogLevel);
                var logger = loggerFactory.CreateLogger("Rnx");

                commandLineSettings.TasksToRun = string.IsNullOrWhiteSpace(taskArgument.Value) ? new[] { DEFAULT_TASK_NAME }
                                                                                               : taskArgument.Values.ToArray();
                logger.LogVerbose($"Current dirctory: {currentDirectory}");
                logger.LogVerbose($"Tasks to run: {string.Join(", ", commandLineSettings.TasksToRun)}");

                string rnxProjectDirectory = PlatformServices.Default.Application.ApplicationBasePath;

                if (rnxProjectDirectoryOption.HasValue())
                {
                    var rnxDirOptionValue = rnxProjectDirectoryOption.Value().TrimEnd('"');
                    logger.LogVerbose($"Rnx project directory specified: {rnxDirOptionValue}");
                    rnxProjectDirectory = Path.GetFullPath(Path.Combine(currentDirectory, rnxDirOptionValue));

                    if (!Directory.Exists(rnxProjectDirectory))
                    {
                        logger.LogError($"Can not find Rnx project directory '{rnxProjectDirectory}'. Please specify a valid directory.");
                        return 1;
                    }
                }

                if(baseDirectoryOption.HasValue())
                {
                    commandLineSettings.BaseDirectory = baseDirectoryOption.Value().TrimEnd('"');
                    logger.LogVerbose($"Base directory specified: {commandLineSettings.BaseDirectory}");
                }
                    
                commandLineSettings.RnxProjectDirectory = rnxProjectDirectory;

                return new RnxApp(commandLineSettings, loggerFactory).Run();
            });

            try
            {
                return rnxApp.Execute(args);
            }
            catch (Exception ex)
            {
                entryLogger.LogCritical($"Task execution error: {ex.Message}");
                return 1;
            }
            finally
            {
                stopwatch.Stop();
                entryLogger.LogVerbose("Total: " + stopwatch.ElapsedMilliseconds + " ms");
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
    }
}