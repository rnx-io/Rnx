using Reliak.IO.Abstractions;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Util.FileSystem;
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
            var elementFactory = GetBufferElementFactory(executionContext);
            var fileSystem = GetFileSystem(executionContext);
            var globMatcher = GetGlobMatcher(executionContext);
            var taskRunTracker = GetTaskRunTracker(executionContext);

            foreach(var file in globMatcher.FindMatches(dirPath, _globPatterns).Where(f => _condition(f.FullPath)))
            {
                if (_onlyChangedFilesSinceLastRun)
                {
                    DateTime lastRunOfParentTaskUtc;

                    if (taskRunTracker.LastRunsOfUserDefinedTasks.TryGetValue(executionContext.UserDefinedTaskName, out lastRunOfParentTaskUtc))
                    {
                        var fi = fileSystem.GetFileInfo(file.FullPath);

                        // if file didn't change since our last run, we ignore it
                        if (fi.LastWriteTimeUtc < lastRunOfParentTaskUtc)
                        {
                            return;
                        }
                    }
                }

                var relativePath = _baseDirectory != null ? Path.Combine(_baseDirectory, file.Stem) : file.Stem;
                var newElement = elementFactory.Create(() => fileSystem.File.OpenRead(file.FullPath));
                newElement.Data.Add(new ReadFileData(file.FullPath));
                newElement.Data.Add(new WriteFileData(relativePath));

                output.Add(newElement);
            }
        }

        protected virtual IBufferElementFactory GetBufferElementFactory(IExecutionContext ctx) => RequireService<IBufferElementFactory>(ctx);
        protected virtual IFileSystem GetFileSystem(IExecutionContext ctx) => RequireService<IFileSystem>(ctx);
        protected virtual IGlobMatcher GetGlobMatcher(IExecutionContext ctx) => RequireService<IGlobMatcher>(ctx);
        protected virtual ITaskRunTracker GetTaskRunTracker(IExecutionContext ctx) => RequireService<ITaskRunTracker>(ctx);
    }
}