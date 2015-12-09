using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Execution
{
    public interface IExecutionContext
    {
        string UserDefinedTaskName { get; }
        string BaseDirectory { get; set; }
        IServiceProvider ServiceProvider { get; }
    }
}