using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Util
{
    public interface IMetaDataReferenceProvider
    {
        IEnumerable<MetadataReference> GetCurrentMetaDataReferences();
    }
}
