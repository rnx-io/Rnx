using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class SimpleWatchTask : RnxTask, IAsyncTask
    {
        public event EventHandler<AsyncTaskCompletedEventArgs> Completed;

        private string _directoryPath;
        private string _simpleFilter;
        private Predicate<FileSystemEventArgs> _advancedFilter;
        private ITask _taskToRun;
        private bool _includeSubdirectories;
        private WatcherChangeTypes _changeType = WatcherChangeTypes.All;
        private Action<FileSystemEventArgs> _onChangeAction;

        public string ExecutionId { get; private set; }

        public SimpleWatchTask(string directoryPath, ITask taskToRun, string simpleFilter = "*.*", bool includeSubdirectories = true)
        {
            _directoryPath = directoryPath;
            _simpleFilter = simpleFilter;
            _taskToRun = taskToRun;
            _includeSubdirectories = includeSubdirectories;
            ExecutionId = Guid.NewGuid().ToString();
        }

        public SimpleWatchTask WhereChangeType(WatcherChangeTypes changeType)
        {
            _changeType = changeType;
            return this;
        }

        public SimpleWatchTask WithAdvancedFilter(Predicate<FileSystemEventArgs> advancedFilter)
        {
            _advancedFilter = advancedFilter;
            return this;
        }

        public SimpleWatchTask OnChange(Action<FileSystemEventArgs> action)
        {
            _onChangeAction = action;
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferFactory = GetService<IBufferFactory>(executionContext);

            Task.Run(() =>
            {
                var resetEvent = new ManualResetEventSlim(false);

                try
                {
                    var onChangeCallback = new Action<FileSystemEventArgs>(e =>
                    {
                        if (_advancedFilter == null || _advancedFilter(e))
                        {
                            _onChangeAction?.Invoke(e);

                            using (var taskToRunOutputBuffer = bufferFactory.Create())
                            {
                                ExecuteTask(_taskToRun, new NullBuffer(), taskToRunOutputBuffer, executionContext);
                            }
                        }
                    });

                    var watcher = new FileSystemWatcher(_directoryPath, _simpleFilter);
                    watcher.IncludeSubdirectories = _includeSubdirectories;
                    watcher.EnableRaisingEvents = true;

                    if (_changeType.HasFlag(WatcherChangeTypes.Changed))
                    {
                        watcher.Changed += (s, e) => onChangeCallback(e);
                    }

                    if (_changeType.HasFlag(WatcherChangeTypes.Created))
                    {
                        watcher.Created += (s, e) => onChangeCallback(e);
                    }

                    if (_changeType.HasFlag(WatcherChangeTypes.Deleted))
                    {
                        watcher.Deleted += (s, e) => onChangeCallback(e);
                    }

                    if (_changeType.HasFlag(WatcherChangeTypes.Renamed))
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