using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rnx.TaskLoader.Compilation
{
    public class DefaultCodeCompiler : ICodeCompiler
    {
        private IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private IMetaDataReferenceProvider _metaDataReferenceProvider;
        private IApplicationEnvironment _applicationEnvironment;

        public DefaultCodeCompiler(IAssemblyLoadContextAccessor assemblyLoadContextAccessor, IMetaDataReferenceProvider metaDataReferenceProvider, IApplicationEnvironment applicationEnvironment)
        {
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _metaDataReferenceProvider = metaDataReferenceProvider;
            _applicationEnvironment = applicationEnvironment;
        }

        public Assembly Compile(params string[] sourceCodes)
        {
            IEnumerable<string> preprocessorSymbols = _applicationEnvironment.Configuration.ToLower().Contains("debug") ? new string[] { "DEBUG" }
                                                                                                                        : Enumerable.Empty<string>();
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: BuildSyntaxTrees(sourceCodes, new CSharpParseOptions(preprocessorSymbols: preprocessorSymbols)),
                references: _metaDataReferenceProvider.GetCurrentMetaDataReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug, warningLevel: 0));

            Assembly compiledAssembly = null;

            using (var ms = new MemoryStream())
            {
                var compilationResult = compilation.Emit(ms);

                if (!compilationResult.Success)
                    throw new CodeCompilationException(string.Join(Environment.NewLine, compilationResult.Diagnostics.Select(f => f.GetMessage()).ToArray()));

                ms.Seek(0, SeekOrigin.Begin);
                compiledAssembly = _assemblyLoadContextAccessor.Default.LoadStream(ms, null);
            }

            return compiledAssembly;
        }

        private IEnumerable<SyntaxTree> BuildSyntaxTrees(string[] sourceCodes, CSharpParseOptions parseOptions)
        {
            return sourceCodes.Select(f => CSharpSyntaxTree.ParseText(f, parseOptions));
        }
    }
}
