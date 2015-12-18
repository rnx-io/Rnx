using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Rnx.Tasks.Core.Control
{
    public class FilterTaskDescriptor : TaskDescriptorBase<FilterTask>
    {
        public Func<IBufferElement, bool> Predicate { get; }

        public FilterTaskDescriptor(Func<IBufferElement, bool> predicate)
        {
            Predicate = predicate;
        }
    }

    public class FilterTask : ControlTask
    {
        private readonly FilterTaskDescriptor _filterTaskDescriptor;

        public FilterTask(FilterTaskDescriptor filterTaskDescriptor)
        {
            _filterTaskDescriptor = filterTaskDescriptor;
        }
        
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements.Where(_filterTaskDescriptor.Predicate))
            {
                output.Add(e);
            }
        }
    }
}