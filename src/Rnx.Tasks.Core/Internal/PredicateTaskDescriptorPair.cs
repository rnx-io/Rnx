using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Internal
{
    internal class PredicateTaskDescriptorPair
    {
        public Func<IBufferElement, bool> Predicate { get; }
        public ITaskDescriptor TaskDescriptor { get; }

        public PredicateTaskDescriptorPair(Func<IBufferElement, bool> predicate, ITaskDescriptor taskDescriptor)
        {
            Predicate = predicate;
            TaskDescriptor = taskDescriptor;
        }
    }
}