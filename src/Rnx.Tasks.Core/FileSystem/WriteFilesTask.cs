using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using System.IO;
using Rnx.Abstractions.Util;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Buffers;

namespace Rnx.Tasks.Core.FileSystem
{
    public class WriteFilesTask : RnxTask
    {
        private string _destinationDirectory;

        public WriteFilesTask(string destinationDirectory)
        {
            _destinationDirectory = destinationDirectory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var baseDir = executionContext.BaseDirectory;

            foreach(var e in input.Elements.Where(f => f.Data.Exists<WriteFileData>()))
            {
                var writeFileData = e.Data.Get<WriteFileData>();
                string outputFilename = null;

                if (writeFileData.IsPathRooted)
                {
                    outputFilename = writeFileData.RelativePath;
                }
                else
                {
                    if (Path.IsPathRooted(_destinationDirectory))
                    {
                        outputFilename = Path.GetFullPath(Path.Combine(_destinationDirectory, writeFileData.RelativePath));
                    }
                    else
                    {
                        outputFilename = Path.GetFullPath(Path.Combine(baseDir, _destinationDirectory, writeFileData.RelativePath));
                    }
                }

                WriteBufferElement(e, outputFilename);
                writeFileData.WrittenFilename = outputFilename;

                output.Add(e);
            }
        }

        private void WriteBufferElement(IBufferElement bufferElement, string outputFilename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(outputFilename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilename));
            }

            if (bufferElement.HasText)
            {
                File.WriteAllText(outputFilename, bufferElement.Text);
            }
            else
            {
                using (var fs = File.OpenWrite(outputFilename))
                {
                    bufferElement.Stream.CopyTo(fs);
                }
            }
        }
    }
}