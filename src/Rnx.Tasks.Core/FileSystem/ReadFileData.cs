using Rnx.Abstractions.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class ReadFileData
    {
        public ReadFileData(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; private set; }
        public string Extension => Path.GetExtension(Filename);
        public string BaseName => Path.GetFileName(Filename);
    }
}
