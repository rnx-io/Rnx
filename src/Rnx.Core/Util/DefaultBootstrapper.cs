using Rnx.Common.Util;
using Microsoft.CodeAnalysis;
using Microsoft.Dnx.Compilation;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rnx.Core.Util
{
    public class DefaultBootstrapper : IBootstrapper
    {
        private MetadataReference[] _metaDataReferences;
        private List<IServiceRegistration> _serviceRegistrations;

        private ILibraryExporter _libraryExporter;
        private IApplicationEnvironment _applicationEnvironment;
        private IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;

        public DefaultBootstrapper(IApplicationEnvironment applicationEnvironment, ILibraryExporter libraryExporter,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor)
        {
            _libraryExporter = libraryExporter;
            _applicationEnvironment = applicationEnvironment;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _serviceRegistrations = new List<IServiceRegistration>();
        }

        public void Initialize()
        {
            _metaDataReferences = _libraryExporter.GetAllExports(_applicationEnvironment.ApplicationName).MetadataReferences.Select(mr =>
            {
                var isRnxAssembly = mr.Name.StartsWith("Rnx.", StringComparison.OrdinalIgnoreCase);

                if (mr is IMetadataFileReference)
                {
                    var filename = ((IMetadataFileReference)mr).Path;

                    if (isRnxAssembly)
                    {
                        AnalyzeAssemblyForServiceRegistrations(_assemblyLoadContextAccessor.Default.LoadFile(filename));
                    }

                    return MetadataReference.CreateFromFile(filename);
                }

                if (mr is IMetadataProjectReference)
                {
                    if (isRnxAssembly)
                    {
                        AnalyzeAssemblyForServiceRegistrations(_assemblyLoadContextAccessor.Default.Load(mr.Name));
                    }

                    var pr = (IMetadataProjectReference)mr;

                    using (var ms = new MemoryStream())
                    {
                        pr.EmitReferenceAssembly(ms);

                        return MetadataReference.CreateFromStream(ms);
                    }
                }

                return null;
            }).Where(f => f != null).ToArray();
        }

        private void AnalyzeAssemblyForServiceRegistrations(Assembly assembly)
        {
            var serviceRegistrationType = assembly.GetExportedTypes()
                                                  .FirstOrDefault(t => typeof(IServiceRegistration).IsAssignableFrom(t) && t.GetTypeInfo().IsClass);
            if (serviceRegistrationType != null)
            {
                var serviceRegistration = (IServiceRegistration)Activator.CreateInstance(serviceRegistrationType);

                _serviceRegistrations.Add(serviceRegistration);
            }
        }

        public IEnumerable<IServiceRegistration> GetServiceRegistrations()
        {
            return _serviceRegistrations;
        }

        public IEnumerable<MetadataReference> GetCurrentMetaDataReferences()
        {
            return _metaDataReferences;
        }
    }
}