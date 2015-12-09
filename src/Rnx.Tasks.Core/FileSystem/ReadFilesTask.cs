using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class ReadFilesTask : RnxTask
    {
        private string[] _globPatterns;
        private Func<string, bool> _condition;
        private string _baseDirectory;
        private bool _onlyChangedFilesSinceLastRun;

        public ReadFilesTask(params string[] globPatterns)
        {
            _globPatterns = globPatterns;
            _condition = new Func<string, bool>(s => true);
        }
        
        public ReadFilesTask Where(Func<string, bool> condition)
        {
            _condition = condition;
            return this;
        }

        public ReadFilesTask WithBase(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
            return this;
        }

        public ReadFilesTask WhereChangedSinceLastRun()
        {
            _onlyChangedFilesSinceLastRun = true;
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var dirPath = executionContext.BaseDirectory;
            var elementFactory = GetService<IBufferElementFactory>(executionContext);
            var filesystem = GetService<IFileSystem>(executionContext);
            var taskRunTracker = GetService<ITaskRunTracker>(executionContext);

            foreach(var file in filesystem.FindFiles(dirPath, _globPatterns).Where(f => _condition(f.FullPath)))
            {
                if (_onlyChangedFilesSinceLastRun)
                {
                    DateTime lastRunOfParentTaskUtc;

                    if (taskRunTracker.LastRuns.TryGetValue(executionContext.UserDefinedTaskName, out lastRunOfParentTaskUtc))
                    {
                        var fi = new FileInfo(file.FullPath);

                        // if file didn't change since our last run, we ignore it
                        if (fi.LastWriteTimeUtc < lastRunOfParentTaskUtc)
                        {
                            return;
                        }
                    }
                }

                var relativePath = _baseDirectory != null ? Path.Combine(_baseDirectory, file.Stem) : file.Stem;
                var newElement = elementFactory.Create(() => File.OpenRead(file.FullPath));
                newElement.Data.Add(new ReadFileData(file.FullPath));
                newElement.Data.Add(new WriteFileData(relativePath));

                output.Add(newElement);
            }
        }
    }
}