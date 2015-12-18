using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnx.Core.Tasks
{
    /// <summary>
    /// Default implementation for the <see cref="IAsyncTaskManager"/>
    /// </summary>
    public class DefaultAsyncTaskManager : IAsyncTaskManager
    {
        private readonly ConcurrentDictionary<AsyncTaskExecutionKey, ManualResetEventSlim> _tasks;
        private readonly Dictionary<AsyncTaskExecutionKey, AsyncTaskCompletedEventArgs> _results;

        public DefaultAsyncTaskManager()
        {
            _tasks = new ConcurrentDictionary<AsyncTaskExecutionKey, ManualResetEventSlim>();
            _results = new Dictionary<AsyncTaskExecutionKey, AsyncTaskCompletedEventArgs>();
        }

        public bool HasUncompletedTasks => _tasks.Any();

        public void RegisterAsyncExecution(IAsyncTask asyncTask, ITaskDescriptor rootTaskDescriptor)
        {
            // we prefix the name of the user defined task name to the execution id, because
            // maybe the same execution id is used in another user defined task and we want
            // the execution ids to be unique
            var key = new AsyncTaskExecutionKey(rootTaskDescriptor, asyncTask.ExecutionId);

            asyncTask.Completed += AsyncTask_Completed;
            _tasks.TryAdd(key, new ManualResetEventSlim(false));
        }

        private void AsyncTask_Completed(object sender, AsyncTaskCompletedEventArgs e)
        {
            var key = new AsyncTaskExecutionKey(e.ExecutionContext.RootTaskDescriptor, e.AsyncTask.ExecutionId);
            ManualResetEventSlim completionEvent;

            if (_tasks.TryRemove(key, out completionEvent))
            {
                _results.Add(key, e);
                completionEvent.Set();
                completionEvent.Dispose();
            }
        }

        public void WaitForTaskCompletion(ITaskDescriptor rootTaskDescriptor)
        {
            var affectedTasks = _tasks.Where(f => f.Key.TaskDescriptor == rootTaskDescriptor);

            foreach (var t in affectedTasks.ToArray())
            {
                t.Value.Wait();
                _results.Remove(t.Key);
            }
        }

        public AsyncTaskCompletedEventArgs WaitForTaskCompletion(ITaskDescriptor rootTaskDescriptor, string executionId)
        {
            var key = new AsyncTaskExecutionKey(rootTaskDescriptor, executionId);
            ManualResetEventSlim completionEvent;

            if( _tasks.TryGetValue(key, out completionEvent) )
            {
                completionEvent.Wait();

                AsyncTaskCompletedEventArgs resultingEventArgs;

                return _results.TryGetValue(key, out resultingEventArgs) ? resultingEventArgs : null;
            }

            return null;
        }

        public void WaitAll()
        {
            while(_tasks.Any())
            {
                _tasks.Values.First().Wait();
            }
        }

        private struct AsyncTaskExecutionKey : IEquatable<AsyncTaskExecutionKey>
        {
            private readonly ITaskDescriptor _taskDescriptor;
            private readonly string _executionId;

            public AsyncTaskExecutionKey(ITaskDescriptor taskDescriptor, string executionId)
            {
                _taskDescriptor = taskDescriptor;
                _executionId = executionId;
            }

            public ITaskDescriptor TaskDescriptor => _taskDescriptor;

            public bool Equals(AsyncTaskExecutionKey other)
            {
                return other._executionId == _executionId && other._taskDescriptor == _taskDescriptor;
            }
        }
    }
}
