using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Control;
using System;

namespace Rnx.Tasks.Core.FileSystem
{
    public class CopyFilesTaskDescriptor : TaskDescriptorBase<CopyFilesTask>
    {
        internal ReadFilesTaskDescriptor ReadFilesTaskDescriptor { get; }
        internal WriteFilesTaskDescriptor WriteFilesTaskDescriptor { get; }

        public CopyFilesTaskDescriptor(string sourceGlobPattern, string destination)
        {
            ReadFilesTaskDescriptor = new ReadFilesTaskDescriptor(sourceGlobPattern);
            WriteFilesTaskDescriptor = new WriteFilesTaskDescriptor(destination);
        }

        public CopyFilesTaskDescriptor Where(Func<string, bool> condition)
        {
            ReadFilesTaskDescriptor.Where(condition);
            return this;
        }

        public CopyFilesTaskDescriptor WithBase(string baseDirectory)
        {
            ReadFilesTaskDescriptor.WithBase(baseDirectory);
            return this;
        }

        public CopyFilesTaskDescriptor WhereChangedSinceLastRun()
        {
            ReadFilesTaskDescriptor.WhereChangedSinceLastRun();
            return this;
        }
    }

    public class CopyFilesTask : RnxTask
    {
        private readonly CopyFilesTaskDescriptor _copyFilesTaskDescriptor;
        private readonly ITaskExecuter _taskExecuter;

        public CopyFilesTask(CopyFilesTaskDescriptor copyFilesTaskDescriptor, ITaskExecuter taskExecuter)
        {
            _copyFilesTaskDescriptor = copyFilesTaskDescriptor;
            _taskExecuter = taskExecuter;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            _taskExecuter.Execute(new SeriesTaskDescriptor(_copyFilesTaskDescriptor.ReadFilesTaskDescriptor, _copyFilesTaskDescriptor.WriteFilesTaskDescriptor),
                                input, output, executionContext);
        }
    }
}