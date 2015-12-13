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
        private ConcurrentDictionary<string, ManualResetEventSlim> _tasks;
        private Dictionary<string, AsyncTaskCompletedEventArgs> _results;

        public DefaultAsyncTaskManager()
        {
            _tasks = new ConcurrentDictionary<string, ManualResetEventSlim>();
            _results = new Dictionary<string, AsyncTaskCompletedEventArgs>();
        }

        public bool HasUncompletedTasks => _tasks.Any();

        public void RegisterAsyncExecution(IAsyncTask asyncTask, string userDefinedTaskName)
        {
            // we prefix the name of the user defined task name to the execution id, because
            // maybe the same execution id is used in another user defined task and we want
            // the execution ids to be unique
            var taskExecutionId = $"{userDefinedTaskName}_{asyncTask.ExecutionId}";

            asyncTask.Completed += AsyncTask_Completed;
            _tasks.TryAdd(taskExecutionId, new ManualResetEventSlim(false));
        }

        private void AsyncTask_Completed(object sender, AsyncTaskCompletedEventArgs e)
        {
            var taskExecutionId = $"{e.ExecutionContext.UserDefinedTaskName}_{e.AsyncTask.ExecutionId}";
            ManualResetEventSlim completionEvent;

            if( _tasks.TryRemove(taskExecutionId, out completionEvent) )
            {
                _results.Add(taskExecutionId, e);
                completionEvent.Set();
                completionEvent.Dispose();
            }
        }

        public void WaitForTaskCompletion(string userDefinedTaskName)
        {
            var affectedTasks = _tasks.Where(f => f.Key.StartsWith($"{userDefinedTaskName}_"));

            foreach (var t in affectedTasks.ToArray())
            {
                t.Value.Wait();
                _results.Remove(t.Key);
            }
        }

        public AsyncTaskCompletedEventArgs WaitForTaskCompletion(string userDefinedTaskName, string executionId)
        {
            executionId = $"{userDefinedTaskName}_{executionId}";
            ManualResetEventSlim completionEvent;

            if( _tasks.TryGetValue(executionId, out completionEvent) )
            {
                completionEvent.Wait();

                AsyncTaskCompletedEventArgs resultingEventArgs;

                return _results.TryGetValue(executionId, out resultingEventArgs) ? resultingEventArgs : null;
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
    }
}
