using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader
{
    public class CodeFileInfo
    {
        public string Filename { get; }
        public string Content { get; }

        public CodeFileInfo(string filename, string content)
        {
            Filename = filename;
            Content = content;
        }
    }
}
