using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reliak.IO.Abstractions;
using Rnx.Util.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader
{
    public class SourceCodeResolver : ISourceCodeResolver
    {
        private readonly IFileSystem _fileSystem;

        public SourceCodeResolver(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<CodeFileInfo> GetCodeFileInfos(string taskCodeFilePath)
        {
            if (!_fileSystem.File.Exists(taskCodeFilePath))
            {
                throw new FileNotFoundException($"Task-file '{taskCodeFilePath}' not found");
            }

            var code = _fileSystem.File.ReadAllText(taskCodeFilePath).Trim();
            yield return new CodeFileInfo(taskCodeFilePath, code);

            using (var sr = new StringReader(code))
            {
                string line = null;

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if(line.Length == 0)
                    {
                        continue;
                    }

                    if (!line.StartsWith("//+"))
                    {
                        break;
                    }

                    var fileToInclude = line.Replace("//+", "").Trim();

                    if (fileToInclude.Length > 0)
                    {
                        fileToInclude = Path.IsPathRooted(fileToInclude) ? fileToInclude : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(taskCodeFilePath), fileToInclude));

                        foreach (var codeFileInfo in GetCodeFileInfos(fileToInclude))
                        {
                            yield return codeFileInfo;
                        }
                    }
                }
            }
        }

        public IEnumerable<string> FindAvailableTaskNames(params string[] sourceCodes)
        {
            var tasks = new List<string>();

            foreach (var code in sourceCodes)
            {
                var treeRoot = CSharpSyntaxTree.ParseText(code).GetRoot();

                // iterate through all public classes that are not nested
                foreach (var configClass in treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>()
                                                    .Where(f => !(f.Parent is ClassDeclarationSyntax) && f.Modifiers.Select(x => x.ToString()).Contains("public")))
                {
                    var publicMethods = configClass.ChildNodes().OfType<MethodDeclarationSyntax>()
                                                                .Where(f => f.Modifiers.Select(x => x.ToString()).Contains("public") && f.ReturnType.ToString().Contains("ITaskDescriptor"));
                    var publicProperties = configClass.ChildNodes().OfType<PropertyDeclarationSyntax>()
                                                                .Where(f => f.Modifiers.Select(x => x.ToString()).Contains("public") && f.Type.ToString().Contains("ITaskDescriptor"));

                    tasks.AddRange(publicMethods.Select(f => f.Identifier.ToString())
                                                .Concat(publicProperties.Select(f => f.Identifier.ToString()))
                                                .Select(f => f));
                }
            }

            return tasks;
        }
    }
}