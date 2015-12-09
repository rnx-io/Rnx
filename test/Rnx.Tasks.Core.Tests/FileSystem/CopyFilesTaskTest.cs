using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Tasks.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class CopyFilesTaskTest
    {
        [Fact]
        public void Test_That_Files_Are_Copied()
        {
            // Arrange
            var testContext = new TestContext("Sample1");
            var tempFolder = Path.Combine(Path.GetTempPath(), "CopyFilesTaskTest");
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);

            var buffer = new BlockingBuffer();
            var task = new CopyFilesTask("wwwroot/assets/js/**/*.js", tempFolder);

            // Act
            taskexecuter.Execute(task, new NullBuffer(), buffer, testContext.ExecutionContext);

            // Assert
            var items = buffer.Elements.ToArray();
            var files = Directory.EnumerateFiles(tempFolder).ToArray();

            Assert.Equal(3, items.Select(f => f.Data.Get<ReadFileData>()).Count(f => new[] { "app.js", "bootstrap.min.js", "jquery.js" }.Contains(f.BaseName)));
            Assert.Equal(3, files.Length);
            var appJs = files.FirstOrDefault(f => Path.GetFileName(f) == "app.js");
            var bootstrapJs = files.FirstOrDefault(f => Path.GetFileName(f) == "bootstrap.min.js");
            var jqueryJs = files.FirstOrDefault(f => Path.GetFileName(f) == "jquery.js");

            Assert.NotNull(appJs);
            Assert.NotNull(bootstrapJs);
            Assert.NotNull(jqueryJs);

            Assert.Equal(File.ReadAllText(items.Select(f => f.Data.Get<ReadFileData>().Filename).First(f => Path.GetFileNameWithoutExtension(f) == "app")),
                        File.ReadAllText(appJs));
            Assert.Equal(File.ReadAllText(items.Select(f => f.Data.Get<ReadFileData>().Filename).First(f => Path.GetFileNameWithoutExtension(f) == "bootstrap.min")),
                        File.ReadAllText(bootstrapJs));
            Assert.Equal(File.ReadAllText(items.Select(f => f.Data.Get<ReadFileData>().Filename).First(f => Path.GetFileNameWithoutExtension(f) == "jquery")),
                        File.ReadAllText(jqueryJs));
        }
    }
}
