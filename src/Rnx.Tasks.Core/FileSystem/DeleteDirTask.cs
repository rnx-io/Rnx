using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using System.IO;
using Rnx.Abstractions.Buffers;

namespace Rnx.Tasks.Core.FileSystem
{
    public class DeleteDirTask : RnxTask
    {
        private string[] _directoryPaths;
        private bool _recursive = true;

        public DeleteDirTask(params string[] directoryPaths)
        {
            _directoryPaths = directoryPaths;
        }

        public DeleteDirTask NonRecursive()
        {
            _recursive = false;
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach (var path in _directoryPaths.Where(f => Directory.Exists(f)))
            {
                Directory.Delete(path, _recursive);
            }
        }
    }
}