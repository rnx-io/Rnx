using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.TaskLoader
{
    public class UserDefinedTaskDescriptor : TaskDescriptorBase<UserDefinedTask>
    {
        public ITaskDescriptor RootTaskDescriptor { get; }
        public string UserDefinedTaskName { get; }

        public UserDefinedTaskDescriptor(string userDefinedTaskName, ITaskDescriptor rootTaskDescriptor)
        {
            UserDefinedTaskName = userDefinedTaskName;
            RootTaskDescriptor = rootTaskDescriptor;
        }
    }

    public sealed class UserDefinedTask : RnxTask
    {
        private readonly UserDefinedTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;

        public UserDefinedTask(UserDefinedTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter, LastRunTaskDecorator lastRunTaskDecorator)
        {
            _taskDescriptor = taskDescriptor;
            _taskExecuter = taskExecuter;
            TaskDecorators = Enumerable.Repeat(lastRunTaskDecorator, 1);
        }

        public ITaskDescriptor TaskDescriptor => _taskDescriptor;
        public override string Name => _taskDescriptor.UserDefinedTaskName;

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            _taskExecuter.Execute(_taskDescriptor.RootTaskDescriptor, input, output, executionContext);
        }
    }
}