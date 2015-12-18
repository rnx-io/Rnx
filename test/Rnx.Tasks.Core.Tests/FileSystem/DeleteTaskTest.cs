using Rnx.Abstractions.Execution;
using Rnx.Tasks.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Buffers;
using Rnx.Util.FileSystem;
using Reliak.IO.Abstractions;
using Microsoft.Extensions.PlatformAbstractions;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class DeleteTaskTest
    {
        private static readonly string SampleDir1 = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Samples", "Sample1");

        private DeleteTask CreateDeleteTask(DeleteTaskDescriptor taskDescriptor)
        {
            return new DeleteTask(taskDescriptor, new DefaultGlobMatcher(), new DefaultFileSystem());
        }

        [Fact]
        public void Test_That_Files_And_Folders_Are_Deleted()
        {
            // Arrange
            var d = new DeleteTaskDescriptor("**/*.{js,txt}");
            var tempFolder = Path.Combine(Path.GetTempPath(), "DeleteTaskTest");
            var extecutionContext = new ExecutionContext(d, tempFolder);
            var task = CreateDeleteTask(d);
                        
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);
            CopyDirectory(SampleDir1, tempFolder);

            // Act
            task.Execute(new NullBuffer(), new NullBuffer(), extecutionContext);
            var files = Directory.EnumerateFiles(tempFolder, "*.*", SearchOption.AllDirectories).ToArray();

            // Assert
            Assert.Equal(1, files.Length);
            Assert.True(files.Any(f => Path.GetFileName(f) == "bar.csv"));
            Assert.False(Directory.Exists(Path.Combine(tempFolder, "wwwroot/assets")));
            Assert.True(Directory.Exists(Path.Combine(tempFolder, "wwwroot")));
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
