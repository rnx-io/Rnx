using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Common.Execution.Decorators;
using System.Linq;
using System.IO;

namespace Rnx.Core.Execution
{
    public class DefaultTaskExecuter : ITaskExecuter, ITaskExecutionDecorator
    {
        public ITaskExecutionDecorator Decoratee { private get; set; }
        public Type ExportType => GetType();

        public void Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            ITaskExecutionDecorator decoratee = this;
            var decorators = executionContext.ServiceProvider.GetServices<ITaskExecutionDecorator>();
            var decoratorService = executionContext.ServiceProvider.GetService<ITaskDecoratorService>();
            var taskSpecificDecorators = decoratorService.FindDecorators(task.GetType());

            decorators = decorators.Where(f => !taskSpecificDecorators.Select(x => x.ExportType).Contains(f.ExportType)).Concat(taskSpecificDecorators);

            foreach (var deco in decorators)
            {
                deco.Decoratee = decoratee;
                decoratee = deco;
            }

            decoratee.Execute(task, input, output, executionContext);
        }

        void ITaskExecutionDecorator.Execute(ITask task, IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            task.Execute(input, output, executionContext);
        }
    }
}