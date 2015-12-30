using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.Control
{
    public delegate IOrderedEnumerable<IBufferElement> OrderByCondition(IEnumerable<IBufferElement> elements);

    public class OrderByTaskDescriptor : TaskDescriptorBase<OrderByTask>
    {
        internal OrderByCondition OrderByCondition { get; }

        public OrderByTaskDescriptor(OrderByCondition orderByCondition)
        {
            OrderByCondition = orderByCondition;
            RequiresCompletedInputBuffer = true;
        }
    }

    public class OrderByTask : ControlTask
    {
        private readonly OrderByTaskDescriptor _taskDescriptor;

        public OrderByTask(OrderByTaskDescriptor taskDescriptor)
        {
            _taskDescriptor = taskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in _taskDescriptor.OrderByCondition(input.Elements))
            {
                output.Add(e);
            }
        }
    }
}