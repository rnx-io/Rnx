using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Tasks.Core.Internal;

namespace Rnx.Tasks.Core.Control
{
    public class SwitchTaskDescriptor : TaskDescriptorBase<SwitchTask>
    {
        internal List<PredicateTaskDescriptorPair> PredicateTaskPairs { get; } = new List<PredicateTaskDescriptorPair>();
        private bool _defaultCalled;

        private readonly Func<IBufferElement, object> _valueSelector;

        public SwitchTaskDescriptor(Func<IBufferElement,object> valueSelector)
        {
            _valueSelector = valueSelector;
        }
        
        public SwitchTaskDescriptor Case(object value, ITaskDescriptor taskDescriptorToRun)
        {
            CheckDefaultCalled();

            var predicate = new Func<IBufferElement, bool>(e => _valueSelector(e) == value);
            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(predicate, taskDescriptorToRun));

            return this;
        }

        public SwitchTaskDescriptor Default(ITaskDescriptor taskDescriptorToRun)
        {
            CheckDefaultCalled();

            PredicateTaskPairs.Add(new PredicateTaskDescriptorPair(e => true, taskDescriptorToRun));
            _defaultCalled = true;

            return this;
        }

        private void CheckDefaultCalled()
        {
            if (_defaultCalled)
            {
                throw new InvalidOperationException("Invalid operation. No more conditions possible after the Default-method was called.");
            }
        }
    }
    
    public class SwitchTask : ControlTask
    {
        private readonly SwitchTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public SwitchTask(SwitchTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
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
