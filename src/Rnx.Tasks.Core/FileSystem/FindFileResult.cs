using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class FindFileResult
    {
        public string BaseDirectory { get; private set; }
        public string Path { get; private set; }
        public string Stem { get; private set; }
        public string FullPath { get; private set; }

        public FindFileResult(string baseDirectory, string path, string stem)
        {
            BaseDirectory = baseDirectory;
            Path = path;
            Stem = stem;
            FullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(BaseDirectory, Path));
        }
    }
}
