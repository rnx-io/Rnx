using Rnx.Common.Buffers;
using Rnx.Common.Util;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class DefaultFileSystem : IFileSystem
    {
        private const string GLOB_EXCLUDE_IDENTIFIER = "!";

        public IEnumerable<FindFileResult> FindFiles(string baseDirectory, params string[] globPatterns)
        {
            foreach(var mi in GetMatcherInfos(baseDirectory, globPatterns))
            {
                var matcher = new Matcher();

                foreach (var includePattern in mi.IncludePatterns)
                {
                    matcher.AddInclude(includePattern);
                }

                foreach (var excludePattern in mi.ExcludePatterns)
                {
                    matcher.AddExclude(excludePattern);
                }

                foreach(var file in matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(mi.BaseDirectory))).Files)
                {
                    yield return new FindFileResult(mi.BaseDirectory, file.Path, file.Stem);
                }
            }
        }

        public void WriteBufferElement(IBufferElement bufferElement, string outputFilename)
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

        public bool IsDirectoryEmpty(string directoryPath)
        {
            return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
        }

        internal static IEnumerable<MatcherInfo> GetMatcherInfos(string directoryPath, params string[] globPatterns)
        {
            globPatterns = ExpandGlobPatterns(globPatterns).ToArray();

            var globInfos = globPatterns.Select(f => {
                var isExclude = f.StartsWith(GLOB_EXCLUDE_IDENTIFIER);
                var pattern = isExclude ? f.Substring(1) : f;
                pattern = Path.IsPathRooted(pattern) ? PathHelper.GetRelativePath(pattern, directoryPath) : pattern;

                return new GlobInfo(pattern, isExclude, Path.GetPathRoot(pattern));
            });

            foreach(var group in globInfos.GroupBy(f => f.PathRoot))
            {
                if( string.IsNullOrWhiteSpace(group.Key) )
                {
                    yield return new MatcherInfo(directoryPath,
                        group.Where(f => !f.IsExclude).Select(f => f.Pattern),
                        group.Where(f => f.IsExclude).Select(f => f.Pattern));
                }
                else
                {
                    yield return new MatcherInfo(group.Key,
                        group.Where(f => !f.IsExclude).Select(f => PathHelper.GetRelativePath(f.Pattern, group.Key)),
                        group.Where(f => f.IsExclude).Select(f => PathHelper.GetRelativePath(f.Pattern, group.Key)));
                }
            }
        }

        internal static IEnumerable<string> ExpandGlobPatterns(params string[] globPatterns)
        {
            return globPatterns.SelectMany(f => Minimatch.DNX.Minimatcher.BraceExpand(f, new Minimatch.DNX.Options()));
        }

        private struct GlobInfo
        {
            public string PathRoot { get; private set; }
            public bool IsExclude { get; private set; }
            public string Pattern { get; private set; }

            public GlobInfo(string pattern, bool isExclude, string pathRoot)
            {
                Pattern = pattern;
                IsExclude = isExclude;
                PathRoot = pathRoot;
            }
        }
    }
}