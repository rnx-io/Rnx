using Microsoft.CodeAnalysis;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Dnx.Compilation;
using System.Diagnostics;

namespace Rnx.TaskLoader.Compilation
{
    public class DefaultMetaDataReferenceProvider : IMetaDataReferenceProvider
    {
        private readonly Lazy<IEnumerable<MetadataReference>> _metaDataReferencesLazy;
        private readonly ILibraryExporter _libraryExporter;
        private readonly IApplicationEnvironment _applicationEnvironment;

        public DefaultMetaDataReferenceProvider(IApplicationEnvironment applicationEnvironment, ILibraryExporter libraryExporter)
        {
            _libraryExporter = libraryExporter;
            _applicationEnvironment = applicationEnvironment;
            _metaDataReferencesLazy = new Lazy<IEnumerable<MetadataReference>>(LoadMetaDataReferences);
        }

        public IEnumerable<MetadataReference> GetCurrentMetaDataReferences() => _metaDataReferencesLazy.Value;

        private IEnumerable<MetadataReference> LoadMetaDataReferences()
        {
            foreach(var mr in _libraryExporter.GetAllExports(_applicationEnvironment.ApplicationName).MetadataReferences)
            {
                if (mr is IMetadataFileReference)
                {
                    var filename = ((IMetadataFileReference)mr).Path;
                    yield return MetadataReference.CreateFromFile(filename);
                }
#if DEBUG // only relevant when testing Rnx in Debugger, because Rnx is referencing other Rnx-projects like Rnx.Abstractions et al.
                else if (Debugger.IsAttached && mr is IMetadataProjectReference)
                {
                    var pr = (IMetadataProjectReference)mr;

                    using (var ms = new MemoryStream())
                    {
                        pr.EmitReferenceAssembly(ms);
                        yield return MetadataReference.CreateFromStream(ms);
                    }
                }
#endif
            }
        }
    }
}