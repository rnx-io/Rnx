using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Execution.Decorators
{
    /// <summary>
    /// Represents a queue that contains all the decorators for task executions
    /// </summary>
    public interface IDecoratorQueue
    {
        /// <summary>
        /// Gets the next decorator from the queue or null if all decorators where retrieved
        /// </summary>
        ITaskDecorator GetNext();
    }
}