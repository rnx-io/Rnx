using Rnx.Common.Tasks;
using Rnx.Tasks.Core.Composite;
using Rnx.Tasks.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExperimentalPlayground
{
    public class RnxConfig
    {
        public ITask DoStuff => new SeriesTask(new ReadFilesTask("*.txt"), new RenameTask(f => { f.Extension = ".bak"; }), new WriteFilesTask("backup"));
    }
}
