using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using System.IO;
using Rnx.Abstractions.Util;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Buffers;
using Reliak.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace Rnx.Tasks.Core.FileSystem
{
    public class WriteFilesTaskDescriptor : TaskDescriptorBase<WriteFilesTask>
    {
        internal string DestinationDirectory;

        public WriteFilesTaskDescriptor(string destinationDirectory)
        {
            DestinationDirectory = destinationDirectory;
        }
    }

    public class WriteFilesTask : RnxTask
    {
        private readonly WriteFilesTaskDescriptor _writeFilesTaskDescriptor;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public WriteFilesTask(WriteFilesTaskDescriptor writeFilesTaskDescriptor, IFileSystem fileSystem)
        {
            _writeFilesTaskDescriptor = writeFilesTaskDescriptor;
            _fileSystem = fileSystem;
            _logger = LoggingContext.Current.LoggerFactory.CreateLogger(nameof(WriteFilesTask));
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var baseDir = executionContext.BaseDirectory;
            var numberOfWrittenFiles = 0;

            foreach(var e in input.Elements)
            {
                if (e.Data.Exists<WriteFileData>())
                {
                    var writeFileData = e.Data.Get<WriteFileData>();
                    string outputFilename = null;

                    if (writeFileData.IsPathRooted)
                    {
                        outputFilename = writeFileData.RelativePath;
                    }
                    else
                    {
                        if (Path.IsPathRooted(_writeFilesTaskDescriptor.DestinationDirectory))
                        {
                            outputFilename = Path.GetFullPath(Path.Combine(_writeFilesTaskDescriptor.DestinationDirectory, writeFileData.RelativePath));
                        }
                        else
                        {
                            outputFilename = Path.GetFullPath(Path.Combine(baseDir, _writeFilesTaskDescriptor.DestinationDirectory, writeFileData.RelativePath));
                        }
                    }

                    WriteBufferElement(e, outputFilename);
                    writeFileData.WrittenFilename = outputFilename;

                    numberOfWrittenFiles++;
                }

                output.Add(e);
            }

            _logger.LogVerbose($"{numberOfWrittenFiles} file(s) written");
        }

        private void WriteBufferElement(IBufferElement bufferElement, string outputFilename)
        {
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(outputFilename)))
            {
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(outputFilename));
            }

            if (bufferElement.HasText)
            {
                _fileSystem.File.WriteAllText(outputFilename, bufferElement.Text);
            }
            else
            {
                using (var fs = _fileSystem.File.OpenWrite(outputFilename))
                {
                    bufferElement.Stream.CopyTo(fs);
                }
            }
        }
    }
}