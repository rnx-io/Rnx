using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Util;

namespace Rnx.Abstractions.Execution
{
    /// <summary>
    /// Default implementation for <see cref="IExecutionContext"/>
    /// </summary>
    public class ExecutionContext : IExecutionContext
    {
        public ITaskDescriptor RootTaskDescriptor { get; }
        public string BaseDirectory { get; }

        public ExecutionContext(ITaskDescriptor rootTaskDescriptor, string baseDirectory)
        {
            RootTaskDescriptor = rootTaskDescriptor;
            BaseDirectory = baseDirectory;
        }
    }
}