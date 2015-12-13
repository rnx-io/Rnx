using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.FileSystem;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Rnx.Tasks.Core.Compression
{
    public class ZipTask : RnxTask
    {
        private string _zipEntryFilePath;

        public ZipTask(string zipEntryFilePath)
        {
            _zipEntryFilePath = zipEntryFilePath;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferElementFactory = GetBufferElementFactory(executionContext);

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

            var newElement = bufferElementFactory.Create(() => outputStream);
            newElement.Data.Add(new WriteFileData(_zipEntryFilePath));

            output.Add(newElement);
        }

        protected virtual IBufferElementFactory GetBufferElementFactory(IExecutionContext ctx) => RequireService<IBufferElementFactory>(ctx);
    }
}