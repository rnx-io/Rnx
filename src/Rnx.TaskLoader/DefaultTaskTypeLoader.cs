using Rnx.Abstractions.Exceptions;
using Rnx.TaskLoader.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rnx.TaskLoader
{
    public class DefaultTaskTypeLoader : ITaskTypeLoader
    {
        private readonly ICodeCompiler _codeCompiler;
        private readonly ISourceCodeResolver _sourceCodeResolver;

        public DefaultTaskTypeLoader(ICodeCompiler codeCompiler, ISourceCodeResolver sourceCodeResolver)
        {
            _codeCompiler = codeCompiler;
            _sourceCodeResolver = sourceCodeResolver;
        }

        public IEnumerable<Type> Load(string taskCodeFilePath)
        {
            var codeInfos = _sourceCodeResolver.GetCodeFileInfos(taskCodeFilePath).ToArray();

            if (!codeInfos.Any())
            {
                throw new RnxException($"No source code files were found for '{taskCodeFilePath}'");
            }
            
            var sourceCodes = codeInfos.Select(f => f.Content).ToArray();
            var compiledAssembly = _codeCompiler.Compile(sourceCodes);
            var taskConfigurationTypes = compiledAssembly.ExportedTypes.Where(f => f.GetTypeInfo().IsPublic && f.GetTypeInfo().IsClass);

            if (!taskConfigurationTypes.Any())
            {
                throw new RnxException($"The provided source code files do not contain a public class");
            }

            return taskConfigurationTypes;
        }
    }
}