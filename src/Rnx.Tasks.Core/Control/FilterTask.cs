using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using System;
using System.Linq;

namespace Rnx.Tasks.Core.Control
{
    public class FilterTask : RnxTask
    {
        private Func<IBufferElement, bool> _predicate;

        public FilterTask(Func<IBufferElement,bool> predicate)
        {
            _predicate = predicate;
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