using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Internal;
using System;
using System.Collections.Generic;

namespace Rnx.Tasks.Core.Control
{
    public class IfTaskDescriptor : TaskDescriptorBase<IfTask>
    {
        internal List<PredicateTaskDescriptorPair> PredicateTaskPairs { get; } = new List<PredicateTaskDescriptorPair>();
        private bool _elseCalled;

        public IfTaskDescriptor(Func<IBufferElement,bool> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
        }

        public IfTaskDescriptor ElseIf(Func<IBufferElement, bool> predicate, ITaskDescriptor taskDescriptorToRun)
        {
            CheckElseCalled();

            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(predicate, taskDescriptorToRun));
            return this;
        }

        public IfTaskDescriptor Else(ITaskDescriptor taskDescriptorToRun)
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

    public class IfTask : ControlTask
    {
        private readonly IfTaskDescriptor _ifTaskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public IfTask(IfTaskDescriptor ifTaskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _ifTaskDescriptor = ifTaskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }
        
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var taskPairExecuter = new PredicateTaskDescriptorPairExecuter(_ifTaskDescriptor.PredicateTaskPairs, _taskExecuter, _bufferFactory);
            taskPairExecuter.Execute(input, output, executionContext);
        }
    }
}