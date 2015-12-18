using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
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
        // Compression
        public static ZipTaskDescriptor Zip(string zipEntryFilePath) => new ZipTaskDescriptor(zipEntryFilePath);
        public static UnzipTaskDescriptor Unzip() => new UnzipTaskDescriptor();

        // Content
        public static ReplaceTextTaskDescriptor Replace(string searchText, string replacement) => new ReplaceTextTaskDescriptor(searchText, replacement);
        public static PrependTextTaskDescriptor Prepend(string textToPrepend) => new PrependTextTaskDescriptor(textToPrepend);
        public static AppendTextTaskDescriptor Append(string textToAppend) => new AppendTextTaskDescriptor(textToAppend);
        public static ConcatTextTaskDescriptor Concat(string targetFilepath, string separator = "") => new ConcatTextTaskDescriptor(targetFilepath, separator);

        // Control
        public static SeriesTaskDescriptor Series(params ITaskDescriptor[] taskDescriptors) => new SeriesTaskDescriptor(taskDescriptors);
        public static ParallelTaskDescriptor Parallel(ParallelTaskOutputStrategy outputStrategy, params ITaskDescriptor[] taskDescriptors)
                                                => new ParallelTaskDescriptor(outputStrategy, taskDescriptors);
        public static ParallelTaskDescriptor ConcatElements(params ITaskDescriptor[] taskDescriptors)
                                                => new ParallelTaskDescriptor(ParallelTaskOutputStrategy.ConcatToInput, taskDescriptors);
        public static FilterTaskDescriptor Filter(Func<IBufferElement,bool> predicate) => new FilterTaskDescriptor(predicate);
        public static IfTaskDescriptor If(Func<IBufferElement,bool> predicate, ITaskDescriptor taskDescriptor) => new IfTaskDescriptor(predicate, taskDescriptor);
        public static SwitchTaskDescriptor Switch(Func<IBufferElement, object> valueSelector) => new SwitchTaskDescriptor(valueSelector);
        public static OrderByTaskDescriptor OrderBy(OrderByCondition orderByCondition) => new OrderByTaskDescriptor(orderByCondition);
        public static BlockWiseTaskDescriptor BlockWise(int blockSize, Func<BlockWiseData, ITaskDescriptor> taskDescriptorToRun, bool requiresDetailedBlockInfo = false) 
                                => new BlockWiseTaskDescriptor(blockSize, taskDescriptorToRun, requiresDetailedBlockInfo);

        // FileSystem
        public static SetFilePathTaskDescriptor SetFilePath(Func<IBufferElement, string> elementFilePath) => new SetFilePathTaskDescriptor(elementFilePath);
        public static CopyFilesTaskDescriptor CopyFiles(string sourceGlobPattern, string destination) => new CopyFilesTaskDescriptor(sourceGlobPattern, destination);
        public static DeleteDirTaskDescriptor DeleteDir(params string[] directoryPaths) => new DeleteDirTaskDescriptor(directoryPaths);
        public static DeleteTaskDescriptor Delete(params string[] globPatterns) => new DeleteTaskDescriptor(globPatterns);
        public static ReadFilesTaskDescriptor ReadFiles(params string[] globPatterns) => new ReadFilesTaskDescriptor(globPatterns);
        public static RenameTaskDescriptor Rename(Action<WriteFileData> action) => new RenameTaskDescriptor(action);
        public static RenameTaskDescriptor Rename(Action<IBufferElement, WriteFileData> action) => new RenameTaskDescriptor(action);
        public static WriteFilesTaskDescriptor WriteFiles(string destinationDirectory) => new WriteFilesTaskDescriptor(destinationDirectory);
        public static SimpleWatchTaskDescriptor SimpleWatch(string directoryPath, ITaskDescriptor taskDescriptorToRun, string simpleFilter = "*.*", bool includeSubdirectories = true)
                                => new SimpleWatchTaskDescriptor(directoryPath, taskDescriptorToRun, simpleFilter, includeSubdirectories);

        // Threading
        public static AsyncTaskDescriptor Async(ITaskDescriptor taskDescriptorToRunAsynchronously, string executionId = null)
                                => new AsyncTaskDescriptor(taskDescriptorToRunAsynchronously, executionId);
        public static AwaitTaskDescriptor Await() => new AwaitTaskDescriptor();
        public static AwaitTaskDescriptor Await(string executionId, Action<AsyncTaskCompletedEventArgs, IBuffer, IBuffer, IExecutionContext> action = null)
                                => new AwaitTaskDescriptor(executionId, action);
    }
}