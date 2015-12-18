using Rnx.Abstractions.Util;
using Rnx.Core.Execution;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rnx.Tasks.Core.FileSystem;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;
using Rnx.Core.Buffers;
using Rnx.Util.FileSystem;
using Reliak.IO.Abstractions;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class ReadFilesTaskTest
    {
        private static readonly string SampleDir1 = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Samples", "Sample1");

        private ReadFilesTask CreateReadFilesTask(ReadFilesTaskDescriptor taskDescriptor)
        {
            return new ReadFilesTask(taskDescriptor, new DefaultGlobMatcher(), new DefaultFileSystem(), new DefaultBufferElementFactory(), new NullTaskRunTracker());
        }

        [Fact]
        public void Test_That_Read_Works_And_Relative_Paths_Are_Correct()
        {
            // Arrange
            var d = new ReadFilesTaskDescriptor("wwwroot/assets/**/*.js", "!wwwroot/assets/**/*.min.js");
            var task = CreateReadFilesTask(d);
            var executionContext = new ExecutionContext(d, SampleDir1);
            var buffer = new BlockingBuffer();

            // Act
            task.Execute(new NullBuffer(), buffer, executionContext);
            buffer.CompleteAdding();
            var results = buffer.Elements.ToArray();

            // Assert
            Assert.Equal(2, results.Length);
            Assert.True(results.All(f => f.Data.Exists<ReadFileData>() && f.Data.Exists<WriteFileData>()));

            var relativePaths = results.Select(f => f.Data.Get<WriteFileData>().RelativePath).ToArray();
            Assert.True(relativePaths.Contains(@"js\app.js"));
            Assert.True(relativePaths.Contains(@"js\jquery.js"));
        }

        [Fact]
        public void Test_That_Base_Directory_Works()
        {
            // Arrange
            var d = new ReadFilesTaskDescriptor("wwwroot/assets/**/*.js", "!wwwroot/assets/**/*.min.js").WithBase("assets");
            var task = CreateReadFilesTask(d);
            var executionContext = new ExecutionContext(d, SampleDir1);
            var buffer = new BlockingBuffer();

            // Act
            task.Execute(new NullBuffer(), buffer, executionContext);
            buffer.CompleteAdding();
            var results = buffer.Elements.ToArray();

            // Assert
            Assert.Equal(2, results.Length);
            Assert.True(results.All(f => f.Data.Exists<ReadFileData>() && f.Data.Exists<WriteFileData>()));

            var relativePaths = results.Select(f => f.Data.Get<WriteFileData>().RelativePath).ToArray();
            Assert.True(relativePaths.Contains(@"assets\js\app.js"));
            Assert.True(relativePaths.Contains(@"assets\js\jquery.js"));
        }

        [Fact]
        public void Test_That_Where_Condition_Works()
        {
            // Arrange
            var d = new ReadFilesTaskDescriptor("wwwroot/assets/**/*.js", "!wwwroot/assets/**/*.min.js").Where(f => Path.GetFileName(f).StartsWith("a"));
            var task = CreateReadFilesTask(d);
            var executionContext = new ExecutionContext(d, SampleDir1);
            var buffer = new BlockingBuffer();

            // Act
            task.Execute(new NullBuffer(), buffer, executionContext);
            buffer.CompleteAdding();
            var results = buffer.Elements.ToArray();

            // Assert
            Assert.Equal(1, results.Length);
            Assert.True(results.All(f => f.Data.Exists<ReadFileData>() && f.Data.Exists<WriteFileData>()));

            var relativePaths = results.Select(f => f.Data.Get<WriteFileData>().RelativePath).ToArray();
            Assert.True(relativePaths.Contains(@"js\app.js"));
        }

        [Fact]
        public void Test_That_Single_File_Works()
        {
            // Arrange
            var d = new ReadFilesTaskDescriptor("wwwroot/assets/js/jquery.js");
            var task = CreateReadFilesTask(d);
            var executionContext = new ExecutionContext(d, SampleDir1);
            var buffer = new BlockingBuffer();

            // Act
            task.Execute(new NullBuffer(), buffer, executionContext);
            buffer.CompleteAdding();
            var results = buffer.Elements.ToArray();

            // Assert
            Assert.Equal(1, results.Length);
            Assert.True(results.All(f => f.Data.Exists<ReadFileData>() && f.Data.Exists<WriteFileData>()));

            var relativePaths = results.Select(f => f.Data.Get<WriteFileData>().RelativePath).ToArray();
            Assert.True(relativePaths.Contains(@"jquery.js"));
        }
    }
}
