using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader
{
    public interface ISourceCodeResolver
    {
        IEnumerable<CodeFileInfo> GetCodeFileInfos(string taskCodeFilePath);
        IEnumerable<string> FindAvailableTaskNames(params string[] sourceCodes);
    }
}
