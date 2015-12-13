using Rnx.Abstractions.Execution.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Execution.Decorators
{
    /// <summary>
    /// Default implementation for <see cref="IDecoratorQueue"/>
    /// </summary>
    public class DecoratorQueue : IDecoratorQueue
    {
        private Queue<ITaskDecorator> _queue;

        public DecoratorQueue(IEnumerable<ITaskDecorator> decorators)
        {
            _queue = new Queue<ITaskDecorator>(decorators);
        }

        public ITaskDecorator GetNext()
        {
            return _queue.Any() ? _queue.Dequeue() : null;
        }
    }
}