using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    internal struct MatcherInfo
    {
        public string BaseDirectory { get; private set; }
        public IEnumerable<string> IncludePatterns { get; private set; }
        public IEnumerable<string> ExcludePatterns { get; private set; }

        public MatcherInfo(string baseDirectory, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            BaseDirectory = baseDirectory;
            IncludePatterns = includePatterns;
            ExcludePatterns = excludePatterns;
        }
    }
}
