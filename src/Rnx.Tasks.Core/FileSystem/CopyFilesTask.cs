using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;
using Rnx.Tasks.Core.Composite;
using Rnx.Abstractions.Util;

namespace Rnx.Tasks.Core.FileSystem
{
    public class CopyFilesTask : RnxTask
    {
        private ReadFilesTask _readFiles;
        private WriteFilesTask _writeFiles;

        public CopyFilesTask(string sourceGlobPattern, string destination)
        {
            _readFiles = new ReadFilesTask(sourceGlobPattern);
            _writeFiles = new WriteFilesTask(destination);
        }

        public CopyFilesTask Where(Func<string, bool> condition)
        {
            _readFiles.Where(condition);
            return this;
        }

        public CopyFilesTask WithBase(string baseDirectory)
        {
            _readFiles.WithBase(baseDirectory);
            return this;
        }

        public CopyFilesTask WhereChangedSinceLastRun()
        {
            _readFiles.WhereChangedSinceLastRun();
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            ExecuteTask(new SeriesTask(_readFiles, _writeFiles), input, output, executionContext);
        }
    }
}