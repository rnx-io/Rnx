using Newtonsoft.Json.Linq;
using Rnx.Common.Configuration;
using Rnx.Common.Exceptions;
using Rnx.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rnx.Core.Configuration
{
    public class DefaultTaskConfigurationTypeLoader : ITaskConfigurationTypeLoader
    {
        private const string DEFAULT_CODE_FILENAME = "Rnx.cs";
        private const string DEFAULT_JSON_FILENAME = "rnx.json";
        private const string JSON_SOURCES_ELEMENT = "sources";

        private ICodeCompiler _codeCompiler;

        public DefaultTaskConfigurationTypeLoader(ICodeCompiler codeCompiler)
        {
            _codeCompiler = codeCompiler;
        }

        public IEnumerable<Type> Load(string rnxProjectDirectory)
        {
            var sourceCodeFilenames = DetermineSourceCodeFilenames(rnxProjectDirectory).ToArray();

            if(!sourceCodeFilenames.Any())
            {
                throw new RnxException($"No source code files were found for the provided Rnx project directory {rnxProjectDirectory}");
            }

            var compiledAssembly = _codeCompiler.CompileFrom(sourceCodeFilenames);
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

            if(File.Exists(rnxCsFilename))
            {
                return Enumerable.Repeat(rnxCsFilename, 1);
            }

            var rnxJsonFilename = Path.Combine(rnxProjectDirectory, DEFAULT_JSON_FILENAME);

            if (File.Exists(rnxJsonFilename))
            {
                var rnxJson = JObject.Parse(File.ReadAllText(rnxJsonFilename));
                var sourceNames = rnxJson[JSON_SOURCES_ELEMENT].Select(f => f.ToString());
                var sourcesFullPath = sourceNames.Select(f => Path.GetFullPath(Path.Combine(rnxProjectDirectory, f)));
                var sourceFiles = new List<string>();

                foreach(var sourcePath in sourcesFullPath)
                {
                    if(File.Exists(sourcePath))
                    {
                        sourceFiles.Add(sourcePath);
                    }
                    else if(Directory.Exists(sourcePath))
                    {
                        sourceFiles.AddRange(Directory.EnumerateFiles(sourcePath, "*.cs", SearchOption.AllDirectories));
                    }
                }

                return sourceFiles;
            }

            return Enumerable.Empty<string>();
        }
    }
}