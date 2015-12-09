using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Util;
using Microsoft.Dnx.Runtime;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Rnx.Common.Tasks
{
    public abstract class RnxTask : ITask
    {
        public abstract void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext);

        public virtual string Name => GetType().Name;
        public override string ToString() => Name;
        
        protected virtual T GetService<T>(IExecutionContext executionContext)
        {
            return (T)executionContext.ServiceProvider.GetService(typeof(T));
        }

        protected void ExecuteTask(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var taskExecuter = GetService<ITaskExecuter>(executionContext);
            taskExecuter.Execute(task, input, output, executionContext);
        }

        protected virtual Project TryGetCallingProject(IExecutionContext executionContext)
        {
            var callingProjectLocator = executionContext.ServiceProvider.GetService<ICallingProjectLocator>();

            Project project;
            return callingProjectLocator.TryGetProject(out project) ? project : null;
        }
    }
}