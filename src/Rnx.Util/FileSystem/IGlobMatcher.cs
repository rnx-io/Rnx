using System.Collections.Generic;

namespace Rnx.Util.FileSystem
{
    public interface IGlobMatcher
    {
        IEnumerable<GlobMatchResult> FindMatches(string baseDirectory, params string[] globPatterns);
    }
}