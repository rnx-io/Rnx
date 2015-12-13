using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Execution.Decorators;
using Rnx.Abstractions.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Rnx.Tasks.Core.Control
{
    public class FilterTask : RnxTask, ITaskDecorationProvider
    {
        private Func<IBufferElement, bool> _predicate;

        public FilterTask(Func<IBufferElement,bool> predicate)
        {
            _predicate = predicate;
        }

        public IEnumerable<ITaskDecorator> TaskDecorators
        {
            get
            {
                yield return new NullLoggingTaskDecorator();
            }
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements.Where(_predicate))
            {
                output.Add(e);
            }
        }
    }
}