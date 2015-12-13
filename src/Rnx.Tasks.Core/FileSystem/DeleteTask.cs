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
    public class DeleteTask : RnxTask
    {
        private string[] _globPatterns;
        private bool _keepEmptyDirectories;
        private Func<string, bool> _condition;

        public DeleteTask(params string[] globPatterns)
        {
            _globPatterns = globPatterns;
            _condition = f => true;
        }

        public DeleteTask KeepEmptyDirectories()
        {
            _keepEmptyDirectories = true;
            return this;
        }

        public DeleteTask Where(Func<string, bool> condition)
        {
            _condition = condition;
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var dirPath = executionContext.BaseDirectory;
            var fileSystem = GetService<IFileSystem>(executionContext);
            var globMatcher = GetService<IGlobMatcher>(executionContext);
            var affectedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in globMatcher.FindMatches(dirPath, _globPatterns).Where(f => _condition(f.FullPath)))
            {
                fileSystem.File.Delete(file.FullPath);

                if (!_keepEmptyDirectories)
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
                if (!fileSystem.Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    fileSystem.Directory.Delete(dir);
                }
            }
        }
    }
}