namespace Rnx.Util.FileSystem
{
    public struct GlobMatchResult
    {
        public string FullPath { get; private set; }
        public string Path { get; private set; }
        public string Stem { get; private set; }

        public GlobMatchResult(string path, string fullPath, string stem)
        {
            Path = path;
            FullPath = fullPath;
            Stem = stem;
        }
    }
}