using Microsoft.Extensions.Logging;
using Reliak.IO.Abstractions;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Abstractions.Util;
using Rnx.Util.FileSystem;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class ReadFilesTaskDescriptor : TaskDescriptorBase<ReadFilesTask>
    {
        public string[] GlobPatterns { get; }
        public Func<string, bool> Condition { get; private set; }
        public string BaseDirectory { get; private set; }
        public bool OnlyChangedFilesSinceLastRun { get; private set; }

        public ReadFilesTaskDescriptor(params string[] globPatterns)
        {
            GlobPatterns = globPatterns;
            Condition = new Func<string, bool>(s => true);
        }

        public ReadFilesTaskDescriptor Where(Func<string, bool> condition)
        {
            Condition = condition;
            return this;
        }

        public ReadFilesTaskDescriptor WithBase(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            return this;
        }

        public ReadFilesTaskDescriptor WhereChangedSinceLastRun()
        {
            OnlyChangedFilesSinceLastRun = true;
            return this;
        }
    }

    public class ReadFilesTask : RnxTask
    {
        private readonly ReadFilesTaskDescriptor _readFilesTaskDescriptor;
        private readonly IGlobMatcher _globMatcher;
        private readonly IFileSystem _fileSystem;
        private readonly IBufferElementFactory _bufferElementFactory;
        private readonly ITaskRunTracker _taskRunTracker;
        private readonly ILogger _logger;

        public ReadFilesTask(ReadFilesTaskDescriptor readFilesTaskDescriptor, IGlobMatcher globMatcher, IFileSystem fileSystem, IBufferElementFactory bufferElementFactory, ITaskRunTracker taskRunTracker)
        {
            _readFilesTaskDescriptor = readFilesTaskDescriptor;
            _globMatcher = globMatcher;
            _fileSystem = fileSystem;
            _bufferElementFactory = bufferElementFactory;
            _taskRunTracker = taskRunTracker;
            _logger = LoggingContext.Current.LoggerFactory.CreateLogger(nameof(ReadFilesTask));
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var dirPath = executionContext.BaseDirectory;
            var matches = _globMatcher.FindMatches(dirPath, _readFilesTaskDescriptor.GlobPatterns).Where(f => _readFilesTaskDescriptor.Condition(f.FullPath)).ToArray();

            if(matches.Length == 0)
            {
                _logger.LogVerbose($"No files found for search query: {string.Join(", ", _readFilesTaskDescriptor.GlobPatterns)}");
            }

            foreach (var file in matches)
            {
                if (_readFilesTaskDescriptor.OnlyChangedFilesSinceLastRun)
                {
                    DateTime lastRunOfParentTaskUtc;

                    if (_taskRunTracker.LastTaskRuns.TryGetValue(executionContext.RootTaskDescriptor, out lastRunOfParentTaskUtc))
                    {
                        var fi = _fileSystem.GetFileInfo(file.FullPath);

                        // if file didn't change since our last run, we ignore it
                        if (fi.LastWriteTimeUtc < lastRunOfParentTaskUtc)
                        {
                            return;
                        }
                    }
                }

                var relativePath = _readFilesTaskDescriptor.BaseDirectory != null ? Path.Combine(_readFilesTaskDescriptor.BaseDirectory, file.Stem) : file.Stem;
                var newElement = _bufferElementFactory.Create(() => _fileSystem.File.OpenRead(file.FullPath));
                newElement.Data.Add(new ReadFileData(file.FullPath));
                newElement.Data.Add(new WriteFileData(relativePath));

                output.Add(newElement);
            }
        }
    }
}