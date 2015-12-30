using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnx.Tasks.Core.Control
{
    public class IfTaskDescriptor : TaskDescriptorBase<IfTask>
    {
        internal List<AllElementsPredicateTaskDescriptorPair> PredicateTaskPairs { get; } = new List<AllElementsPredicateTaskDescriptorPair>();
        private bool _elseCalled;

        public IfTaskDescriptor(Func<IBufferElement[], bool> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            PredicateTaskPairs.Add(new AllElementsPredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
            RequiresCompletedInputBuffer = true;
        }

        public IfTaskDescriptor ElseIf(Func<IBufferElement[], bool> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            CheckElseCalled();

            PredicateTaskPairs.Add(new AllElementsPredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
            return this;
        }

        public IfTaskDescriptor Else(ITaskDescriptor taskDescriptorToRun)
        {
            CheckElseCalled();

            PredicateTaskPairs.Add(new AllElementsPredicateTaskDescriptorPair(t => true, taskDescriptorToRun));
            _elseCalled = true;
            return this;
        }

        private void CheckElseCalled()
        {
            if (_elseCalled)
            {
                throw new InvalidOperationException("Invalid operation. No more conditions possible after the Else-method was called.");
            }
        }
    }

    public class IfTask : ControlTask
    {
        private readonly IfTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;

        public IfTask(IfTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter)
        {
            _taskDescriptor = taskDescriptor;
            _taskExecuter = taskExecuter;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var allElements = input.Elements.ToArray();

            foreach(var predicateTaskPair in _taskDescriptor.PredicateTaskPairs)
            {
                if(predicateTaskPair.Predicate(allElements))
                {
                    _taskExecuter.Execute(predicateTaskPair.TaskDescriptor, input, output, executionContext);
                }
            }
        }
    }
}