using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Internal;
using System;
using System.Collections.Generic;

namespace Rnx.Tasks.Core.Control
{
    public class IfElementTaskDescriptor : TaskDescriptorBase<IfElementTask>
    {
        internal List<PredicateTaskDescriptorPair> PredicateTaskPairs { get; } = new List<PredicateTaskDescriptorPair>();
        private bool _elseCalled;

        public IfElementTaskDescriptor(Func<IBufferElement,bool> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
        }

        public IfElementTaskDescriptor ElseIf(Func<IBufferElement, bool> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            CheckElseCalled();

            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
            return this;
        }

        public IfElementTaskDescriptor Else(ITaskDescriptor taskDescriptorToRun)
        {
            CheckElseCalled();

            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(t => true, taskDescriptorToRun));
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

    public class IfElementTask : ControlTask
    {
        private readonly IfElementTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public IfElementTask(IfElementTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _taskDescriptor = taskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }
        
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var taskPairExecuter = new PredicateTaskDescriptorPairExecuter(_taskDescriptor.PredicateTaskPairs, _taskExecuter, _bufferFactory);
            taskPairExecuter.Execute(input, output, executionContext);
        }
    }
}