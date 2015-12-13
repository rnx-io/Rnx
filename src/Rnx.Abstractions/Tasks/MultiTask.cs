using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Rnx.Abstractions.Execution.Decorators;
using System;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Base class for all tasks that act as a composite task, i.e. which are responsible for
    /// managing multiple sub-tasks
    /// </summary>
    public abstract class MultiTask : RnxTask, ITaskDecorationProvider
    {
        /// <summary>
        /// The <see cref="ITask"/>-instances that should be executed
        /// </summary>
        public ITask[] Tasks { get; private set; }

        public virtual IEnumerable<ITaskDecorator> TaskDecorators
        {
            get
            {
                yield return new NullLoggingTaskDecorator();
            }
        }

        public MultiTask(ITask[] tasks)
        {
            Tasks = tasks ?? new ITask[0];
        }
    }
}