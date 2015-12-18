using Rnx.Abstractions.Execution.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Base class for all tasks that act as a composite task, i.e. which are responsible for
    /// managing multiple sub-tasks
    /// </summary>
    public abstract class MultiTaskDescriptor<TTask> : TaskDescriptorBase<TTask>
    {
        /// <summary>
        /// The <see cref="ITaskDescriptor"/>-instances that should be executed
        /// </summary>
        public ITaskDescriptor[] TaskDescriptors { get; private set; }
        
        public MultiTaskDescriptor(ITaskDescriptor[] taskDescriptors)
        {
            TaskDescriptors = taskDescriptors ?? new ITaskDescriptor[0];
        }
    }
}