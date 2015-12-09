using Microsoft.Dnx.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Util
{
    public interface ICallingProjectLocator
    {
        bool TryGetProject(out Project project);
    }

    public class DefaultCallingProjectLocator : ICallingProjectLocator
    {
        private string _containingDirectory;

        public DefaultCallingProjectLocator(string containingDirectory)
        {
            _containingDirectory = containingDirectory;
        }

        public bool TryGetProject(out Project project)
        {
            return Project.TryGetProject(Path.Combine(_containingDirectory, Project.ProjectFileName), out project);
        }
    }

    public class NullCallingProjectLocator : ICallingProjectLocator
    {
        public bool TryGetProject(out Project project)
        {
            project = null;
            return false;
        }
    }
}