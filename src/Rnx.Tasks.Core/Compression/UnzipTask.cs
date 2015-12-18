using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Tasks.Core.FileSystem;
using System.IO.Compression;
using Rnx.Abstractions.Tasks;

namespace Rnx.Tasks.Core.Compression
{
    public class UnzipTaskDescriptor : TaskDescriptorBase<UnzipTask>
    { }

    public class UnzipTask : RnxTask
    {
        private readonly IBufferElementFactory _bufferElementFactory;

        public UnzipTask(IBufferElementFactory bufferElementFactory)
        {
            _bufferElementFactory = bufferElementFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            Parallel.ForEach(input.ElementsPartitioner, e =>
            {
                using (var archive = new ZipArchive(e.Stream, ZipArchiveMode.Read, false))
                {
                    // the name of a directory-entry in a zip file is string.Empty. Ignore them.
                    foreach (var zipEntry in archive.Entries.Where(f => !string.Equals(f.Name, string.Empty)))
                    {
                        var outputStream = new MemoryStream();
                        var newElement = _bufferElementFactory.Create(() => outputStream);
                        newElement.Data.Add(new WriteFileData(zipEntry.FullName));

                        using (var zipEntryStream = zipEntry.Open())
                        {
                            zipEntryStream.CopyTo(outputStream);
                        }

                        // move cursor to the beginning, so the next tasks can read stream content
                        outputStream.Seek(0, SeekOrigin.Begin);

                        output.Add(newElement);
                    }
                }
            });
        }
    }
}