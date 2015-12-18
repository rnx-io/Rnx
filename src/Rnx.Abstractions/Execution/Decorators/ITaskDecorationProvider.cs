using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Execution.Decorators
{
    /// <summary>
    /// Should be implemented by <see cref="ITask"/>s to signal that task decorators
    /// are available for the task
    /// </summary>
    public interface ITaskDecorationProvider
    {
        /// <summary>
        /// Gets or sets the task decorators that should be used for the task
        /// </summary>
        IEnumerable<ITaskDecorator> TaskDecorators { get; set; }
    }
}