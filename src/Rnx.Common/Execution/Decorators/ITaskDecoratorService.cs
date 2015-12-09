using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Execution.Decorators
{
    public interface ITaskDecoratorService
    {
        ITaskDecoratorService Decorate(Type taskType, ITaskExecutionDecorator decorator);
        IEnumerable<ITaskExecutionDecorator> FindDecorators(Type taskType);
    }
}