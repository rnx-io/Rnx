using Rnx.Common.Execution;
using Rnx.Tasks.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Common.Buffers;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class DeleteTaskTest
    {
        [Fact]
        public void Test_That_Files_And_Folders_Are_Deleted()
        {
            // Arrange
            var testContext = new TestContext("Sample1");
            var tempFolder = Path.Combine(Path.GetTempPath(), "DeleteTaskTest");
            var task = new DeleteTask("**/*.{js,txt}");
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();
                        
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);
            CopyDirectory(testContext.SampleDirectory, tempFolder);
            testContext.ExecutionContext.BaseDirectory = tempFolder;

            // Act
            taskexecuter.Execute(task, new NullBuffer(), new NullBuffer(), testContext.ExecutionContext);
            var files = Directory.EnumerateFiles(tempFolder, "*.*", SearchOption.AllDirectories).ToArray();

            // Assert
            Assert.Equal(1, files.Length);
            var barCsv = files.FirstOrDefault(f => Path.GetFileName(f) == "bar.csv");

            Assert.NotNull(barCsv);
            Assert.False(Directory.Exists(Path.Combine(tempFolder, "wwwroot/assets")));
            Assert.True(Directory.Exists(Path.Combine(tempFolder, "wwwroot")));
        }

        [Fact]
        public void Test_That_Files_And_Folders_Are_Deleted_With_Condition()
        {
            // Arrange
            var testContext = new TestContext("Sample1");
            var tempFolder = Path.Combine(Path.GetTempPath(), "DeleteTaskTest");
            var task = new DeleteTask("**/*.{js,txt}").Where(f => Path.GetFileNameWithoutExtension(f) != "app");
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);
            CopyDirectory(testContext.SampleDirectory, tempFolder);
            testContext.ExecutionContext.BaseDirectory = tempFolder;

            // Act
            taskexecuter.Execute(task, new NullBuffer(), new NullBuffer(), testContext.ExecutionContext);
            var files = Directory.EnumerateFiles(tempFolder, "*.*", SearchOption.AllDirectories).ToArray();

            // Assert
            Assert.Equal(2, files.Length);
            var barCsv = files.FirstOrDefault(f => Path.GetFileName(f) == "bar.csv");
            var appJs = files.FirstOrDefault(f => Path.GetFileName(f) == "app.js");

            Assert.NotNull(barCsv);
            Assert.NotNull(appJs);

            Assert.False(Directory.Exists(Path.Combine(tempFolder, "wwwroot/assets/img")));
            Assert.True(Directory.Exists(Path.Combine(tempFolder, "wwwroot/assets/js")));
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }
    }
}
