using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Tasks.Core.FileSystem;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;
using Rnx.Core.Buffers;
using Reliak.IO.Abstractions;
using Microsoft.Extensions.PlatformAbstractions;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class WriteFilesTaskTest
    {
        private static readonly string SampleDir1 = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Samples", "Sample1");

        private WriteFilesTask CreateReadFilesTask(WriteFilesTaskDescriptor taskDescriptor)
        {
            return new WriteFilesTask(taskDescriptor, new DefaultFileSystem());
        }

        [Fact]
        public void Test_That_Files_Are_Written()
        {
            // Arrange
            var tempFolder = Path.Combine(Path.GetTempPath(), "WriteFilesTaskTest");

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);

            var bufferElementFactory = new DefaultBufferElementFactory();
            var bufferElements = new List<IBufferElement>();
            bufferElements.Add(bufferElementFactory.Create("text1"));
            bufferElements.Add(bufferElementFactory.Create("text2"));

            for (int i = 0; i < bufferElements.Count; ++i)
            {
                bufferElements[i].Data.Add(new WriteFileData($"testfolder/{i}.txt"));
            }

            var d = new WriteFilesTaskDescriptor(Path.Combine(tempFolder, "output"));
            var task = CreateReadFilesTask(d);
            var executionContext = new ExecutionContext(d, tempFolder);
            var inputBuffer = new BlockingBuffer();
            var outputBuffer = new BlockingBuffer();

            foreach (var e in bufferElements)
            {
                inputBuffer.Add(e);
            }

            inputBuffer.CompleteAdding();

            // Act
            task.Execute(inputBuffer, outputBuffer, executionContext);
            outputBuffer.CompleteAdding();

            // Assert
            var items = outputBuffer.Elements.ToArray();
            var files = Directory.EnumerateFiles(tempFolder, "*.*", SearchOption.AllDirectories).ToArray();

            Assert.Equal(2, files.Length);
            Assert.True(files.Any(f => Path.GetFileName(f) == "0.txt"));
            Assert.True(files.Any(f => Path.GetFileName(f) == "1.txt"));

            Assert.Equal("text1", File.ReadAllText(items.Select(f => f.Data.Get<WriteFileData>().WrittenFilename).First(f => Path.GetFileNameWithoutExtension(f) == "0")));
            Assert.Equal("text2", File.ReadAllText(items.Select(f => f.Data.Get<WriteFileData>().WrittenFilename).First(f => Path.GetFileNameWithoutExtension(f) == "1")));
        }
    }
}
