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

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class WriteFilesTaskTest
    {
        [Fact]
        public void Test_That_Files_Are_Written()
        {
            // Arrange
            var testContext = new TestContext("Sample1");
            var tempFolder = Path.Combine(Path.GetTempPath(), "WriteFilesTaskTest");
            var taskexecuter = testContext.ServiceProvider.GetService<ITaskExecuter>();

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);

            var bufferElementFactory = testContext.ServiceProvider.GetService<IBufferElementFactory>();
            var bufferElement1 = bufferElementFactory.Create("text1");
            var bufferElement2 = bufferElementFactory.Create("text2");

            var bufferElements = new List<IBufferElement>();
            bufferElements.Add(bufferElement1);
            bufferElements.Add(bufferElement2);

            for (int i = 0; i < bufferElements.Count; ++i)
            {
                bufferElements[i].Data.Add(new WriteFileData($"testfolder/{i}.txt"));
            }

            var task = new WriteFilesTask(Path.Combine(tempFolder, "output"));
            var inputBuffer = new BlockingBuffer();
            var outputBuffer = new BlockingBuffer();

            foreach (var e in bufferElements)
            {
                inputBuffer.Add(e);
            }

            inputBuffer.CompleteAdding();

            // Act
            taskexecuter.Execute(task, inputBuffer, outputBuffer, testContext.ExecutionContext);

            // Assert
            var items = outputBuffer.Elements.ToArray();
            var files = Directory.EnumerateFiles(tempFolder, "*.*", SearchOption.AllDirectories).ToArray();

            Assert.Equal(2, files.Length);
            var firstFile = files.FirstOrDefault(f => Path.GetFileName(f) == "0.txt");
            var secondFile = files.FirstOrDefault(f => Path.GetFileName(f) == "1.txt");

            Assert.NotNull(firstFile);
            Assert.NotNull(secondFile);

            Assert.Equal("text1", File.ReadAllText(items.Select(f => f.Data.Get<WriteFileData>().WrittenFilename).First(f => Path.GetFileNameWithoutExtension(f) == "0")));
            Assert.Equal("text2", File.ReadAllText(items.Select(f => f.Data.Get<WriteFileData>().WrittenFilename).First(f => Path.GetFileNameWithoutExtension(f) == "1")));
        }
    }
}
