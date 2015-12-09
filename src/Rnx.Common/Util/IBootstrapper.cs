using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Rnx.Common.Util
{
    public interface IBootstrapper : IMetaDataReferenceProvider
    {
        void Initialize();
        IEnumerable<IServiceRegistration> GetServiceRegistrations();
    }
}
