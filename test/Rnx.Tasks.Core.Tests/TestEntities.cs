using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Core.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rnx.Tasks.Core.Tests
{
    public class AllowEvenNumbersTaskDescriptor : TaskDescriptorBase<AllowEvenNumbersTask>
    {}

    public class AllowEvenNumbersTask : RnxTask
    {
        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                int i;

                if(int.TryParse(e.Text, out i) && i % 2 == 0)
                {
                    output.Add(e);
                }
            }

            output.CompleteAdding();
        }
    }

    public class SleepTaskDescriptor : TaskDescriptorBase<SleepTask>
    {
        public int SleepDurationInMillisecondsPerElement { get; }

        public SleepTaskDescriptor(int sleepDurationInMillisecondsPerElement)
        {
            SleepDurationInMillisecondsPerElement = sleepDurationInMillisecondsPerElement;
        }
    }

    public class SleepTask : RnxTask
    {
        private SleepTaskDescriptor _sleepTaskDescriptor;

        public SleepTask(SleepTaskDescriptor sleepTaskDescriptor)
        {
            _sleepTaskDescriptor = sleepTaskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach (var e in input.Elements)
            {
                System.Threading.Thread.Sleep(_sleepTaskDescriptor.SleepDurationInMillisecondsPerElement);
                output.Add(e);
            }

            output.CompleteAdding();
        }
    }

    public class TestTaskExecuter : ITaskExecuter
    {
        private readonly List<DescriptorTaskPair> _pairs = new List<DescriptorTaskPair>();

        public ITaskDescriptor[] Descriptors => _pairs.Select(f => f.Descriptor).ToArray();

        public DescriptorTaskPair Add<TTaskDescriptor>(TTaskDescriptor taskDescriptor, Func<TTaskDescriptor, ITask> taskFactory)
            where TTaskDescriptor : ITaskDescriptor
        {
            var pair = new DescriptorTaskPair { Descriptor = taskDescriptor, Task = taskFactory(taskDescriptor) };
            _pairs.Add(pair);
            return pair;
        }

        public void Execute(ITaskDescriptor taskDescriptor, IBuffer input, IBuffer output, IExecutionContext context)
        {
            var res = _pairs.Single(f => f.Descriptor == taskDescriptor);
            res.Task.Execute(input, output, context);
        }
    }

    public class DescriptorTaskPair
    {
        public ITaskDescriptor Descriptor { get; set; }
        public ITask Task { get; set; }
    }

    public class NullTaskRunTracker : ITaskRunTracker
    {
        public Dictionary<ITaskDescriptor, DateTime> LastTaskRuns { get; } = new Dictionary<ITaskDescriptor, DateTime>();
    }
}
