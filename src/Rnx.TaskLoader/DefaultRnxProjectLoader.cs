using Newtonsoft.Json.Linq;
using Reliak.IO.Abstractions;
using Rnx.Abstractions.Exceptions;
using Rnx.TaskLoader.Compilation;
using Rnx.Util.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rnx.TaskLoader
{
    public class DefaultRnxProjectLoader : IRnxProjectLoader
    {
        private const string DEFAULT_CODE_FILENAME = "Rnx.cs";
        private const string DEFAULT_PROJECT_FILENAME = "rnx.json";
        private const string JSON_SOURCES_ELEMENT = "sources";

        private readonly ICodeCompiler _codeCompiler;
        private readonly IFileSystem _fileSystem;
        private readonly IGlobMatcher _globMatcher;

        public DefaultRnxProjectLoader(ICodeCompiler codeCompiler, IFileSystem fileSystem, IGlobMatcher globMatcher)
        {
            _codeCompiler = codeCompiler;
            _fileSystem = fileSystem;
            _globMatcher = globMatcher;
        }

        public IEnumerable<Type> Load(string rnxProjectDirectory)
        {
            var sourceCodeFilenames = DetermineSourceCodeFilenames(rnxProjectDirectory).ToArray();

            if(!sourceCodeFilenames.Any())
            {
                throw new RnxException($"No source code files were found for the provided Rnx project directory {rnxProjectDirectory}");
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

        private IEnumerable<string> DetermineSourceCodeFilenames(string rnxProjectDirectory)
        {
            var rnxCsFilename = Path.Combine(rnxProjectDirectory, DEFAULT_CODE_FILENAME);

            if(_fileSystem.File.Exists(rnxCsFilename))
            {
                return Enumerable.Repeat(rnxCsFilename, 1);
            }

            var rnxJsonFilename = Path.Combine(rnxProjectDirectory, DEFAULT_PROJECT_FILENAME);

            if (_fileSystem.File.Exists(rnxJsonFilename))
            {
                var rnxJson = JObject.Parse(_fileSystem.File.ReadAllText(rnxJsonFilename));
                var sourceGlobs = rnxJson[JSON_SOURCES_ELEMENT].Select(f => f.ToString()).ToArray();

                return _globMatcher.FindMatches(rnxProjectDirectory, sourceGlobs).Select(f => f.FullPath);
            }

            return Enumerable.Empty<string>();
        }
    }
}