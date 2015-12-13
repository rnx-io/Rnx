using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Composite;
using Rnx.Tasks.Core.Compression;
using Rnx.Tasks.Core.Content;
using Rnx.Tasks.Core.Control;
using Rnx.Tasks.Core.FileSystem;
using Rnx.Tasks.Core.Threading;
using System;

namespace Rnx.Tasks.Core
{
    public static class Tasks
    {
        // Composite
        public static SeriesTask Series(params ITask[] tasks) => new SeriesTask(tasks);
        public static ParallelTask Parallel(params ITask[] tasks) => new ParallelTask(tasks);

        // Compression
        public static ZipTask Zip(string zipEntryFilePath) => new ZipTask(zipEntryFilePath);
        public static UnzipTask Unzip() => new UnzipTask();

        // Content
        public static ReplaceTask Replace(string searchText, string replacement) => new ReplaceTask(searchText, replacement);
        public static PrependTask Prepend(string textToPrepend) => new PrependTask(textToPrepend);
        public static AppendTask Append(string textToAppend) => new AppendTask(textToAppend);
        public static ConcatTask Concat(string targetFilepath, string separator = "") => new ConcatTask(targetFilepath, separator);

        // Control
        public static FilterTask Filter(Func<IBufferElement,bool> predicate) => new FilterTask(predicate);
        public static IfTask If(Predicate<IBufferElement> predicate, ITask taskToRun) => new IfTask(predicate, taskToRun);

        // FileSystem
        public static CopyFilesTask CopyFiles(string sourceGlobPattern, string destination) => new CopyFilesTask(sourceGlobPattern, destination);
        public static DeleteDirTask DeleteDir(params string[] directoryPaths) => new DeleteDirTask(directoryPaths);
        public static DeleteTask Delete(params string[] globPatterns) => new DeleteTask(globPatterns);
        public static ReadFilesTask ReadFiles(params string[] globPatterns) => new ReadFilesTask(globPatterns);
        public static RenameTask Rename(Action<WriteFileData> action) => new RenameTask(action);
        public static RenameTask Rename(Action<IBufferElement, WriteFileData> action) => new RenameTask(action);
        public static WriteFilesTask WriteFiles(string destinationDirectory) => new WriteFilesTask(destinationDirectory);
        public static SimpleWatchTask SimpleWatch(string directoryPath, ITask taskToRun, string simpleFilter = "*.*", bool includeSubdirectories = true)
                                => new SimpleWatchTask(directoryPath, taskToRun, simpleFilter, includeSubdirectories);

        // Threading
        public static AsyncTask Async(ITask taskToRunAsynchronously, string executionId = null, bool requiresClonedElements = false)
                                => new AsyncTask(taskToRunAsynchronously, executionId, requiresClonedElements);
        public static AwaitTask Await() => new AwaitTask();
        public static AwaitTask Await(string executionId, Action<AsyncTaskCompletedEventArgs, IBuffer, IBuffer, IExecutionContext> action = null)
                                => new AwaitTask(executionId, action);
    }
}