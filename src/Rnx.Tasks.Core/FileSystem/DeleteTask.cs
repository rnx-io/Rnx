using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Common.Execution;
using Rnx.Common.Util;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Rnx.Common.Buffers;

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
            var filesystem = GetService<IFileSystem>(executionContext);
            var affectedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in filesystem.FindFiles(dirPath, _globPatterns).Where(f => _condition(f.FullPath)))
            {
                File.Delete(file.FullPath);

                if (!_keepEmptyDirectories)
                {
                    var path = Path.GetDirectoryName(file.Path);

                    while (path.Length > 0)
                    {
                        affectedDirectories.Add(Path.GetFullPath(Path.Combine(file.BaseDirectory, path)));
                        path = Path.GetDirectoryName(path);
                    }
                }
            }

            foreach (var dir in affectedDirectories.OrderByDescending(f => f.Length))
            {
                if (filesystem.IsDirectoryEmpty(dir))
                {
                    Directory.Delete(dir);
                }
            }
        }
    }
}
