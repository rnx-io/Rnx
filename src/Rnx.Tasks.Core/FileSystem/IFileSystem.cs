using Rnx.Common.Buffers;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<FindFileResult> FindFiles(string baseDirectory, params string[] globPatterns);
        void WriteBufferElement(IBufferElement bufferElement, string outputFilename);
        bool IsDirectoryEmpty(string directoryPath);
    }
}
