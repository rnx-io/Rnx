using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class SimpleWatchTaskDescriptor : TaskDescriptorBase<SimpleWatchTask>
    {
        public string DirectoryPath { get; }
        public string SimpleFilter { get; }
        public Predicate<FileSystemEventArgs> AdvancedFilter { get; private set; }
        public ITaskDescriptor TaskDescriptorToRun { get; }
        public bool IncludeSubdirectories { get; }
        public WatcherChangeTypes ChangeType { get; private set; } = WatcherChangeTypes.All;
        public Action<FileSystemEventArgs> OnChangeAction { get; private set; }

        public string ExecutionId { get; private set; }

        public SimpleWatchTaskDescriptor(string directoryPath, ITaskDescriptor taskDescriptorToRun, string simpleFilter = "*.*", bool includeSubdirectories = true)
        {
            DirectoryPath = directoryPath;
            SimpleFilter = simpleFilter;
            TaskDescriptorToRun = taskDescriptorToRun;
            IncludeSubdirectories = includeSubdirectories;
            ExecutionId = Guid.NewGuid().ToString();
        }

        public SimpleWatchTaskDescriptor WhereChangeType(WatcherChangeTypes changeType)
        {
            ChangeType = changeType;
            return this;
        }

        public SimpleWatchTaskDescriptor WithAdvancedFilter(Predicate<FileSystemEventArgs> advancedFilter)
        {
            AdvancedFilter = advancedFilter;
            return this;
        }

        public SimpleWatchTaskDescriptor OnChange(Action<FileSystemEventArgs> action)
        {
            OnChangeAction = action;
            return this;
        }
    }

    public class SimpleWatchTask : RnxTask, IAsyncTask
    {
        public event EventHandler<AsyncTaskCompletedEventArgs> Completed;

        private readonly SimpleWatchTaskDescriptor _simpleWatchTaskDescriptor;
        private readonly IBufferFactory _bufferFactory;
        private readonly ITaskExecuter _taskExecuter;

        

        public SimpleWatchTask(SimpleWatchTaskDescriptor simpleWatchTaskDescriptor, IBufferFactory bufferFactory, ITaskExecuter taskExecuter)
        {
            _simpleWatchTaskDescriptor = simpleWatchTaskDescriptor;
            _bufferFactory = bufferFactory;
            _taskExecuter = taskExecuter;
            ExecutionId = Guid.NewGuid().ToString();
        }

        public string ExecutionId { get; }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            Task.Run(() =>
            {
                var resetEvent = new ManualResetEventSlim(false);

                try
                {
                    var onChangeCallback = new Action<FileSystemEventArgs>(e =>
                    {
                        if (_simpleWatchTaskDescriptor.AdvancedFilter == null || _simpleWatchTaskDescriptor.AdvancedFilter(e))
                        {
                            _simpleWatchTaskDescriptor.OnChangeAction?.Invoke(e);

                            using (var taskToRunOutputBuffer = _bufferFactory.Create())
                            {
                                _taskExecuter.Execute(_simpleWatchTaskDescriptor.TaskDescriptorToRun, new NullBuffer(), taskToRunOutputBuffer, executionContext);
                            }
                        }
                    });

                    var watcher = new FileSystemWatcher(_simpleWatchTaskDescriptor.DirectoryPath, _simpleWatchTaskDescriptor.SimpleFilter);
                    watcher.IncludeSubdirectories = _simpleWatchTaskDescriptor.IncludeSubdirectories;
                    watcher.EnableRaisingEvents = true;

                    if (_simpleWatchTaskDescriptor.ChangeType.HasFlag(WatcherChangeTypes.Changed))
                    {
                        watcher.Changed += (s, e) => onChangeCallback(e);
                    }

                    if (_simpleWatchTaskDescriptor.ChangeType.HasFlag(WatcherChangeTypes.Created))
                    {
                        watcher.Created += (s, e) => onChangeCallback(e);
                    }

                    if (_simpleWatchTaskDescriptor.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
                    {
                        watcher.Deleted += (s, e) => onChangeCallback(e);
                    }

                    if (_simpleWatchTaskDescriptor.ChangeType.HasFlag(WatcherChangeTypes.Renamed))
                    {
                        watcher.Renamed += (s, e) => onChangeCallback(e);
                    }

                    resetEvent.Wait();
                }
                catch (Exception ex)
                {
                    Completed?.Invoke(this, new AsyncTaskCompletedEventArgs(this, executionContext, ex.Message));
                }
            });
        }
    }
}