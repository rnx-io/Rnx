using Reliak.FileSystemGlobbingExtensions;
using System.Collections.Generic;
using System.IO;

namespace Rnx.Util.FileSystem
{
    public class DefaultGlobMatcher : IGlobMatcher
    {
        public IEnumerable<GlobMatchResult> FindMatches(string baseDirectory, params string[] globPatterns)
        {
            foreach(var match in GlobMatcher.FindMatches(baseDirectory, globPatterns))
            {
                yield return new GlobMatchResult(match.Path, Path.GetFullPath(Path.Combine(baseDirectory, match.Path)), match.Stem);
            }
        }
    }
}