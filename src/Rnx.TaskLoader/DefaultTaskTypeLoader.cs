using Reliak.IO.Abstractions;
using Rnx.Abstractions.Exceptions;
using Rnx.TaskLoader.Compilation;
using Rnx.Util.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rnx.TaskLoader
{
    public class DefaultTaskTypeLoader : ITaskTypeLoader
    {
        private readonly ICodeCompiler _codeCompiler;
        private readonly IFileSystem _fileSystem;
        private readonly IGlobMatcher _globMatcher;

        public DefaultTaskTypeLoader(ICodeCompiler codeCompiler, IFileSystem fileSystem, IGlobMatcher globMatcher)
        {
            _codeCompiler = codeCompiler;
            _fileSystem = fileSystem;
            _globMatcher = globMatcher;
        }

        public IEnumerable<Type> Load(string baseDirectory, params string[] searchPatterns)
        {
            var sourceCodeFilenames = DetermineSourceCodeFilenames(baseDirectory, searchPatterns).ToArray();

            if(!sourceCodeFilenames.Any())
            {
                throw new RnxException($"No source code files were found for '{string.Join(", ", searchPatterns)}'");
            }

            var sourceCodes = sourceCodeFilenames.Select(f => _fileSystem.File.ReadAllText(f)).ToArray();
            var compiledAssembly = _codeCompiler.Compile(sourceCodes);
            var taskConfigurationTypes = compiledAssembly.ExportedTypes.Where(f => f.GetTypeInfo().IsPublic && f.GetTypeInfo().IsClass);

            if (!taskConfigurationTypes.Any())
            {
                throw new RnxException($"The provided source code files do not contain a public class");
            }

            return taskConfigurationTypes;
        }

        private IEnumerable<string> DetermineSourceCodeFilenames(string baseDirectory, params string[] searchPatterns)
        {
            return _globMatcher.FindMatches(baseDirectory, searchPatterns).Select(f => f.FullPath);
        }
    }
}