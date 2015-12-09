using Rnx.Common.Execution.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Execution.Decorators
{
    public class DefaultTaskDecoratorService : ITaskDecoratorService
    {
        private Dictionary<Type, List<ITaskExecutionDecorator>> _decorators;

        public DefaultTaskDecoratorService()
        {
            _decorators = new Dictionary<Type, List<ITaskExecutionDecorator>>();
        }

        public ITaskDecoratorService Decorate(Type taskType, ITaskExecutionDecorator decorator)
        {
            List<ITaskExecutionDecorator> decorators;

            if(!_decorators.TryGetValue(taskType, out decorators))
            {
                decorators = new List<ITaskExecutionDecorator>();
                _decorators.Add(taskType, decorators);
            }

            decorators.Add(decorator);

            return this;
        }

        public IEnumerable<ITaskExecutionDecorator> FindDecorators(Type taskType)
        {
            List<ITaskExecutionDecorator> decorators;
            return _decorators.TryGetValue(taskType, out decorators) ? decorators : Enumerable.Empty<ITaskExecutionDecorator>();
        }
    }
}