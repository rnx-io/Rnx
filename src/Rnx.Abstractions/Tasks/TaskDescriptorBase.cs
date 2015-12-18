using Rnx.Abstractions.Execution.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Base class for all <see cref="ITaskDescriptor"/>s
    /// </summary>
    /// <typeparam name="TTask">The type of the task to which this task descriptor belongs</typeparam>
    public abstract class TaskDescriptorBase<TTask> : ITaskDescriptor
    {
        public virtual bool RequiresCompletedInputBuffer { get; protected set; } = false;

        public Type TaskType => typeof(TTask);
    }
}