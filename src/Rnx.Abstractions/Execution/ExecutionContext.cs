using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Execution
{
    /// <summary>
    /// Default implementation for <see cref="IExecutionContext"/>
    /// </summary>
    public class ExecutionContext : IExecutionContext
    {
        public string UserDefinedTaskName { get; set; }
        public string BaseDirectory { get; set; }
        public IServiceProvider ServiceProvider { get; private set; }

        public ExecutionContext(string userDefinedTaskName, string baseDirectory, IServiceProvider serviceProvider)
        {
            UserDefinedTaskName = userDefinedTaskName;
            BaseDirectory = baseDirectory;
            ServiceProvider = serviceProvider;
        }
    }
}