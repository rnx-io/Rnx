using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reliak.IO.Abstractions;
using Rnx.Util.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader
{
    public class TaskNameResolver
    {
        private readonly IGlobMatcher _globMatcher;
        private readonly IFileSystem _fileSystem;

        public TaskNameResolver(IGlobMatcher globMatcher, IFileSystem fileSystem)
        {
            _globMatcher = globMatcher;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> FindAvailableTaskNames(string baseDirectory, params string[] searchPatterns)
        {
            var codeFileFilenames = _globMatcher.FindMatches(baseDirectory, searchPatterns).Select(f => f.FullPath);
            var tasks = new List<string>();

            foreach (var code in codeFileFilenames.Select(f => _fileSystem.File.ReadAllText(f)))
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