using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.FileSystem;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System;

namespace Rnx.Tasks.Core.Compression
{
    public class ZipTaskDescriptor : TaskDescriptorBase<ZipTask>
    {
        public string ZipEntryFilePath { get; private set; }

        public ZipTaskDescriptor(string zipEntryFilePath)
        {
            ZipEntryFilePath = zipEntryFilePath;
        }
    }

    public class ZipTask : RnxTask
    {
        private readonly IBufferElementFactory _bufferElementFactory;
        private readonly ZipTaskDescriptor _taskDescriptor;

        public ZipTask(ZipTaskDescriptor taskDescriptor, IBufferElementFactory bufferElementFactory)
        {
            _taskDescriptor = taskDescriptor;
            _bufferElementFactory = bufferElementFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var outputStream = new MemoryStream();

            using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                foreach (var e in input.Elements.Where(f => f.Data.Exists<WriteFileData>()))
                {
                    var relativePath = e.Data.Get<WriteFileData>().RelativePath;
                    var zipEntry = archive.CreateEntry(relativePath);

                    using (var zipEntryStream = zipEntry.Open())
                    {
                        using (var elementStream = e.Stream)
                        {
                            elementStream.CopyTo(zipEntryStream);
                        }
                    }
                }
            }

            // move cursor to the beginning, so the next tasks can read stream content
            outputStream.Seek(0, SeekOrigin.Begin);

            var newElement = _bufferElementFactory.Create(() => outputStream);
            newElement.Data.Add(new WriteFileData(_taskDescriptor.ZipEntryFilePath));

            output.Add(newElement);
        }
    }
}