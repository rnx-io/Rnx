using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using System.IO;
using Rnx.Abstractions.Buffers;
using Reliak.IO.Abstractions;

namespace Rnx.Tasks.Core.FileSystem
{
    public class DeleteDirTaskDescriptor : TaskDescriptorBase<DeleteDirTask>
    {
        internal string[] DirectoryPaths { get; }
        internal bool Recursive { get; private set; } = true;

        public DeleteDirTaskDescriptor(params string[] directoryPaths)
        {
            DirectoryPaths = directoryPaths;
        }

        public DeleteDirTaskDescriptor NonRecursive()
        {
            Recursive = false;
            return this;
        }
    }

    public class DeleteDirTask : RnxTask
    {
        private readonly DeleteDirTaskDescriptor _deleteDirTaskDescriptor;
        private readonly IFileSystem _fileSystem;

        public DeleteDirTask(DeleteDirTaskDescriptor deleteDirTaskDescriptor, IFileSystem fileSystem)
        {
            _deleteDirTaskDescriptor = deleteDirTaskDescriptor;
            _fileSystem = fileSystem;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach (var path in _deleteDirTaskDescriptor.DirectoryPaths.Where(f => _fileSystem.Directory.Exists(f)))
            {
                _fileSystem.Directory.Delete(path, _deleteDirTaskDescriptor.Recursive);
            }
        }
    }
}