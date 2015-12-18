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
        /// <param name="rootTaskDescriptor">The rootTaskDescriptor under which this asynchronous task runs</param>
        void RegisterAsyncExecution(IAsyncTask asyncTask, ITaskDescriptor rootTaskDescriptor);

        /// <summary>
        /// Waits for the completion of all asynchronous tasks for a specific user-defined task name
        /// </summary>
        /// <param name="rootTaskDescriptor">The rootTaskDescriptor under which this asynchronous task runs</param>
        void WaitForTaskCompletion(ITaskDescriptor rootTaskDescriptor);

        /// <summary>
        /// Waits for the completion of an asynchronous task
        /// </summary>
        /// <param name="rootTaskDescriptor">The rootTaskDescriptor under which this asynchronous task runs</param>
        /// <param name="executionId">Identifier of the asynchronous task</param>
        AsyncTaskCompletedEventArgs WaitForTaskCompletion(ITaskDescriptor rootTaskDescriptor, string executionId);

        /// <summary>
        /// Waits for all asynchronous tasks of all user-defined tasks to finish
        /// </summary>
        void WaitAll();
    }
}