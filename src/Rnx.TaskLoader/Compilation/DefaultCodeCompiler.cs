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
                try
                {
                    // use cached assembly
                    return _assemblyLoadContextAccessor.Default.LoadFile(tempAssembly);
                }
                catch // if loading somehow fails, ignore exception and compile source code
                { }
            }
            
            // clear all previously compiled, temporary assemblies
            foreach (var filename in _fileSystem.Directory.GetFiles(tempPath))
            {
                _fileSystem.File.Delete(filename);
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: Guid.NewGuid().ToString(),
                syntaxTrees: BuildSyntaxTrees(sourceCodes),
                references: _metaDataReferenceProvider.GetCurrentMetaDataReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, warningLevel: 0));

            try
            {
                using (var ms = _fileSystem.File.Open(tempAssembly, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    var compilationResult = compilation.Emit(ms);

                    if (!compilationResult.Success)
                    {
                        throw new CodeCompilationException(string.Join(Environment.NewLine, compilationResult.Diagnostics.Select(f => f.GetMessage()).ToArray()));
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    return _assemblyLoadContextAccessor.Default.LoadStream(ms, null);
                }
            }
            catch(Exception ex)
            {
                try
                {
                    // delete the non-successful build
                    _fileSystem.File.Delete(tempAssembly);
                }
                catch { }

                throw ex;
            }
        }

        private IEnumerable<SyntaxTree> BuildSyntaxTrees(string[] sourceCodes)
        {
            return sourceCodes.Select(f => CSharpSyntaxTree.ParseText(f));
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