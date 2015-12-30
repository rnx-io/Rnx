using Reliak.IO.Abstractions;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Util.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rnx.Tasks.Core.FileSystem
{
    public class DeleteTaskDescriptor : TaskDescriptorBase<DeleteTask>
    {
        internal string[] GlobPatterns { get; }
        internal bool ShouldKeepEmptyDirectories { get; private set; }
        internal Func<string, bool> Condition { get; private set; }

        public DeleteTaskDescriptor(params string[] globPatterns)
        {
            GlobPatterns = globPatterns;
            Condition = f => true;
        }

        public DeleteTaskDescriptor KeepEmptyDirectories()
        {
            ShouldKeepEmptyDirectories = true;
            return this;
        }

        public DeleteTaskDescriptor Where(Func<string, bool> condition)
        {
            Condition = condition;
            return this;
        }
    }

    public class DeleteTask : RnxTask
    {
        private readonly DeleteTaskDescriptor _deleteTaskDescriptor;
        private readonly IGlobMatcher _globMatcher;
        private readonly IFileSystem _fileSystem;

        public DeleteTask(DeleteTaskDescriptor deleteTaskDescriptor, IGlobMatcher globMatcher, IFileSystem fileSystem)
        {
            _deleteTaskDescriptor = deleteTaskDescriptor;
            _globMatcher = globMatcher;
            _fileSystem = fileSystem;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var dirPath = executionContext.BaseDirectory;
            var affectedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in _globMatcher.FindMatches(dirPath, _deleteTaskDescriptor.GlobPatterns).Where(f => _deleteTaskDescriptor.Condition(f.FullPath)))
            {
                _fileSystem.File.Delete(file.FullPath);

                if (!_deleteTaskDescriptor.ShouldKeepEmptyDirectories)
                {
                    var path = Path.GetDirectoryName(file.Path);

                    while (path.Length > 0)
                    {
                        affectedDirectories.Add(Path.GetFullPath(Path.Combine(dirPath, path)));
                        path = Path.GetDirectoryName(path);
                    }
                }
            }

            foreach (var dir in affectedDirectories.OrderByDescending(f => f.Length))
            {
                // Delete if directory is empty
                if (!_fileSystem.Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    _fileSystem.Directory.Delete(dir);
                }
            }
        }
    }
}