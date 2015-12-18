using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Is used to specify the type of the actual task and all the information that this task will need to perform.
    /// This indirection is required to let users creat task descriptors, but not the actual tasks themselves.
    /// </summary>
    public interface ITaskDescriptor
    {
        /// <summary>
        /// The type of the actual task that will use the information provided by this task descriptor
        /// </summary>
        Type TaskType { get; }

        /// <summary>
        /// Specifies whether a task will need all elements from the buffer before it can start executing.
        /// This is for example the case for an order-by task. This task most likely needs all elements before
        /// the ordering can happen. This flag is helpful to decide whether a thread for this task should be spawned
        /// when the first element was added to the input buffer or only when all elements are in the buffer.
        /// </summary>
        bool RequiresCompletedInputBuffer { get; }
    }
}