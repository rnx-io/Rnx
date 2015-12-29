﻿namespace Rnx
{
    public class CommandLineSettings
    {
        public const int DEFAULT_LOG_LEVEL = (int)Microsoft.Extensions.Logging.LogLevel.Information;

        public string[] TasksToRun { get; set; }
        public string[] TaskSourceCodeGlobPaths { get; set; }
        public string BaseDirectory { get; set; }
        public bool IsSilent { get; set; }
        public int LogLevel { get; set; } = DEFAULT_LOG_LEVEL;
    }
}