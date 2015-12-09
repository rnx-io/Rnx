using Rnx.Common.Util;
using Rnx.Core.Execution;
using Rnx.Core.Util;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rnx.Tasks.Core.FileSystem;
using Rnx.Common.Buffers;
using Rnx.Common.Execution;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class RenameTaskTest
    {
        [Fact]
        public void Test_That_Extension_Rename_Works()
        {
            // Arrange
            var testContext = new TestContext("Sample1");
            var readFilesTask = new ReadFilesTask("wwwroot/assets/**/*.js", "!wwwroot/assets/**/*.min.js"); // all js files, except minified files (*.min.js)
            var renameTask = new RenameTask(f => { f.Extension = ".min.js"; });
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();
            var renameInputBuffer = new BlockingBuffer();
            var renameOutputBuffer = new BlockingBuffer();

            // Act
            taskexecuter.Execute(readFilesTask, new NullBuffer(), renameInputBuffer, testContext.ExecutionContext);
            taskexecuter.Execute(renameTask, renameInputBuffer, renameOutputBuffer, testContext.ExecutionContext);

            // Assert
            var results = renameOutputBuffer.Elements.ToArray();

            Assert.Equal(2, results.Length);
            Assert.True(results.All(f => f.Data.Exists<ReadFileData>() && f.Data.Exists<WriteFileData>()));

            var relativePaths = results.Select(f => f.Data.Get<WriteFileData>().RelativePath).ToArray();
            Assert.True(relativePaths.Contains(@"js\app.min.js"));
            Assert.True(relativePaths.Contains(@"js\jquery.min.js"));
        }

        [Fact]
        public void Test_That_Stem_Rename_Works()
        {
            // Arrange
            var testContext = new TestContext("Sample1");
            var readFilesTask = new ReadFilesTask("wwwroot/assets/**/*.js", "!wwwroot/assets/**/*.min.js"); // all js files, except minified files (*.min.js)
            //var renameTask = new RenameTask().Extension(".min.js").Stem(f => "my" + f.Get<WriteFileSupport>().Stem);
            var renameTask = new RenameTask(f => { f.Extension = ".min.js"; f.Stem = "my" + f.Stem; });
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();
            var renameInputBuffer = new BlockingBuffer();
            var renameOutputBuffer = new BlockingBuffer();

            // Act
            taskexecuter.Execute(readFilesTask, new NullBuffer(), renameInputBuffer, testContext.ExecutionContext);
            taskexecuter.Execute(renameTask, renameInputBuffer, renameOutputBuffer, testContext.ExecutionContext);

            // Assert
            var results = renameOutputBuffer.Elements.ToArray();

            Assert.Equal(2, results.Length);
            Assert.True(results.All(f => f.Data.Exists<ReadFileData>() && f.Data.Exists<WriteFileData>()));

            var relativePaths = results.Select(f => f.Data.Get<WriteFileData>().RelativePath).ToArray();
            Assert.True(relativePaths.Contains(@"js\myapp.min.js"));
            Assert.True(relativePaths.Contains(@"js\myjquery.min.js"));
        }
    }
}
