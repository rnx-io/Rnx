using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Execution
{
    public class ExecutionContext : IExecutionContext
    {
        public string UserDefinedTaskName { get; set; }
        public string BaseDirectory { get; set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public Dictionary<Type, IServiceProvider> TaskSpecificServiceProviders { get; private set; }

        public ExecutionContext(string userDefinedTaskName, string baseDirectory, IServiceProvider serviceProvider)
        {
            UserDefinedTaskName = userDefinedTaskName;
            BaseDirectory = baseDirectory;
            ServiceProvider = serviceProvider;
            TaskSpecificServiceProviders = new Dictionary<Type, IServiceProvider>();
        }

        public IExecutionContext Clone()
        {
            var clone = new ExecutionContext(UserDefinedTaskName, BaseDirectory, ServiceProvider);

            foreach(var kvp in TaskSpecificServiceProviders)
            {
                clone.TaskSpecificServiceProviders.Add(kvp.Key, kvp.Value);
            }

            return clone;
        }
    }
}