using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Rnx.TaskLoader.Compilation
{
    /// <summary>
    /// Responsible for providing required meta data references,
    /// which will be required later to compile all code files from the Rnx project
    /// </summary>
    public interface IMetaDataReferenceProvider
    {
        /// <summary>
        /// Retrieve all meta data references
        /// </summary>
        IEnumerable<MetadataReference> GetCurrentMetaDataReferences();
    }
}