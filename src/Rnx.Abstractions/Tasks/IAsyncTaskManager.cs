namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Manages the execution of asynchronous tasks
    /// </summary>
    public interface IAsyncTaskManager
    {
        /// <summary>
        /// Returns whether there are any asynchronous running tasks
        /// </summary>
        bool HasUncompletedTasks { get; }

        /// <summary>
        /// Registers an asynchronous task for future tracking
        /// </summary>
        /// <param name="asyncTask">The asynchronous task</param>
        /// <param name="userDefinedTaskName">The name of the user-defined task under which this asynchronous task runs</param>
        void RegisterAsyncExecution(IAsyncTask asyncTask, string userDefinedTaskName);

        /// <summary>
        /// Waits for the completion of all asynchronous tasks for a specific user-defined task name
        /// </summary>
        /// <param name="userDefinedTaskName">The name of the user-defined task under which this asynchronous task runs</param>
        void WaitForTaskCompletion(string userDefinedTaskName);

        /// <summary>
        /// Waits for the completion of an asynchronous task
        /// </summary>
        /// <param name="userDefinedTaskName">The name of the user-defined task under which this asynchronous task runs</param>
        /// <param name="executionId">Identifier of the asynchronous task</param>
        AsyncTaskCompletedEventArgs WaitForTaskCompletion(string userDefinedTaskName, string executionId);

        /// <summary>
        /// Waits for all asynchronous tasks of all user-defined tasks to finish
        /// </summary>
        void WaitAll();
    }
}