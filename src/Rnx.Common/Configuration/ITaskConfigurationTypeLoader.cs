using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Configuration
{
    public interface ITaskConfigurationTypeLoader
    {
        IEnumerable<Type> Load(string rnxProjectDirectory);
    }
}
