using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rnx.Core.Tasks
{
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

        public void RegisterAsyncExecution(IAsyncTask asyncTask, IExecutionContext taskExecutionContext)
        {
            // we prefix the name of the user defined task name to the execution id, because
            // maybe the same execution id is used in another user defined task and we want
            // the execution ids to be unique
            var taskExecutionId = $"{taskExecutionContext.UserDefinedTaskName}_{asyncTask.ExecutionId}";

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

        /// <summary>
        /// It will be waited till all async tasks of this user-defined tasks are complete
        /// </summary>
        public void WaitForTaskCompletion(IExecutionContext taskExecutionContext)
        {
            var affectedTasks = _tasks.Where(f => f.Key.StartsWith($"{taskExecutionContext.UserDefinedTaskName}_"));

            foreach (var t in affectedTasks.ToArray())
            {
                t.Value.Wait();
                _results.Remove(t.Key);
            }
        }

        /// <summary>
        /// Waits for async tasks from a specified task (taskExecutionContext).
        /// </summary>
        public AsyncTaskCompletedEventArgs WaitForTaskCompletion(IExecutionContext taskExecutionContext, string executionId)
        {
            executionId = $"{taskExecutionContext.UserDefinedTaskName}_{executionId}";
            ManualResetEventSlim completionEvent;

            if( _tasks.TryGetValue(executionId, out completionEvent) )
            {
                completionEvent.Wait();

                AsyncTaskCompletedEventArgs resultingEventArgs;

                return _results.TryGetValue(executionId, out resultingEventArgs) ? resultingEventArgs : null;
            }

            return null;
        }

        /// <summary>
        /// Waits for the completion of all async task from all user defined tasks
        /// </summary>
        public void WaitAll()
        {
            while(_tasks.Any())
            {
                _tasks.Values.First().Wait();
            }
        }
    }
}
