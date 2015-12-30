using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.PlatformAbstractions;
using Reliak.IO.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Rnx.TaskLoader.Compilation
{
    public class DefaultCodeCompiler : ICodeCompiler
    {
        private const string UNIQUE_TEMP_FOLDER = "0699ba33d9e9498486f4cb988b90ef11";

        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IMetaDataReferenceProvider _metaDataReferenceProvider;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IFileSystem _fileSystem;

        public DefaultCodeCompiler(IAssemblyLoadContextAccessor assemblyLoadContextAccessor, IMetaDataReferenceProvider metaDataReferenceProvider,
                IApplicationEnvironment applicationEnvironment, IFileSystem fileSystem)
        {
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _metaDataReferenceProvider = metaDataReferenceProvider;
            _applicationEnvironment = applicationEnvironment;
            _fileSystem = fileSystem;
        }

        public Assembly Compile(params string[] sourceCodes)
        {
            var combinedCode = string.Join("", sourceCodes.OrderBy(f => f).ToArray());
            var sha1 = MakeSHA1(combinedCode);
            var tempPath = GetTempPath();
            var tempAssembly = Path.Combine(tempPath, sha1);

            // check whether the exact source code was previously compiled
            if (_fileSystem.File.Exists(tempAssembly))
            {
                // use cached assembly
                using (var fs = _fileSystem.File.OpenRead(tempAssembly))
                {
                    return _assemblyLoadContextAccessor.Default.LoadStream(fs, null);
                }
            }
            else
            {
                // clear all previously compiled, temporary assemblies
                foreach(var filename in _fileSystem.Directory.GetFiles(tempPath))
                {
                    _fileSystem.File.Delete(filename);
                }
            }

            IEnumerable<string> preprocessorSymbols = _applicationEnvironment.Configuration.ToLower().Contains("debug") ? new string[] { "DEBUG" }
                                                                                                                        : Enumerable.Empty<string>();
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: Guid.NewGuid().ToString(),
                syntaxTrees: BuildSyntaxTrees(sourceCodes, new CSharpParseOptions(preprocessorSymbols: preprocessorSymbols)),
                references: _metaDataReferenceProvider.GetCurrentMetaDataReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug, warningLevel: 0));

            Assembly compiledAssembly = null;

            using(var ms = _fileSystem.File.Open(tempAssembly, FileMode.CreateNew, FileAccess.ReadWrite))
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

        // gets the temporary path to cache compiled assemblies
        // the path is unique based on the current _applicationEnvironment.ApplicationBasePath
        private string GetTempPath()
        {
            var topDir = Path.Combine(Path.GetTempPath(), UNIQUE_TEMP_FOLDER);
            
            if(!_fileSystem.Directory.Exists(topDir))
            {
                _fileSystem.Directory.CreateDirectory(topDir);
            }

            var appDir = Path.Combine(topDir, _applicationEnvironment.ApplicationBasePath.GetHashCode().ToString());

            if (!_fileSystem.Directory.Exists(appDir))
            {
                _fileSystem.Directory.CreateDirectory(appDir);
            }

            return appDir;
        }

        private static string MakeSHA1(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}