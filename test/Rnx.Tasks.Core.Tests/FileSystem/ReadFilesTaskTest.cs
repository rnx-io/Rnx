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
using Rnx.Common.Execution;
using Rnx.Common.Buffers;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class ReadFilesTaskTest
    {
        [Fact]
        public void Test_That_Read_Works_And_Relative_Paths_Are_Correct()
        {
            // Arrange
            var testContext = new TestContext("Sample1");
            var task = new ReadFilesTask("wwwroot/assets/**/*.js", "!wwwroot/assets/**/*.min.js"); // all js files, except minified files (*.min.js)
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();

            // Act
            var buffer = new BlockingBuffer();
            taskexecuter.Execute(task, new NullBuffer(), buffer, testContext.ExecutionContext);
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
            var testContext = new TestContext("Sample1");
            var task = new ReadFilesTask("wwwroot/assets/**/*.js", "!wwwroot/assets/**/*.min.js").WithBase("assets");
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();

            // Act
            var buffer = new BlockingBuffer();
            taskexecuter.Execute(task, new NullBuffer(), buffer, testContext.ExecutionContext);
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
            var testContext = new TestContext("Sample1");
            var task = new ReadFilesTask("wwwroot/assets/**.js", "!wwwroot/assets/**.min.js").Where(f => Path.GetFileName(f).StartsWith("a"));
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();

            // Act
            var buffer = new BlockingBuffer();
            taskexecuter.Execute(task, new NullBuffer(), buffer, testContext.ExecutionContext);
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
            var testContext = new TestContext("Sample1");
            var task = new ReadFilesTask("wwwroot/assets/js/jquery.js");
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();

            // Act
            var buffer = new BlockingBuffer();
            taskexecuter.Execute(task, new NullBuffer(), buffer, testContext.ExecutionContext);
            var results = buffer.Elements.ToArray();

            // Assert
            Assert.Equal(1, results.Length);
            Assert.True(results.All(f => f.Data.Exists<ReadFileData>() && f.Data.Exists<WriteFileData>()));

            var relativePaths = results.Select(f => f.Data.Get<WriteFileData>().RelativePath).ToArray();
            Assert.True(relativePaths.Contains(@"jquery.js"));
        }
    }
}
