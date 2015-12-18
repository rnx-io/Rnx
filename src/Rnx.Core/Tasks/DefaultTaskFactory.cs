using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Tasks
{
    public class DefaultTaskFactory : ITaskFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultTaskFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITask Create(ITaskDescriptor taskDescriptor)
        {
            return (ITask)ActivatorUtilities.CreateInstance(new TempServiceProvider(_serviceProvider, taskDescriptor), taskDescriptor.TaskType);
        }

        private class TempServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _parentServiceProvider;
            private readonly object[] _objects;

            public TempServiceProvider(IServiceProvider parentServiceProvider, params object[] objects)
            {
                _parentServiceProvider = parentServiceProvider;
                _objects = objects;
            }

            public object GetService(Type serviceType)
            {
                var matchingObject = _objects.FirstOrDefault(f => f.GetType() == serviceType);

                return matchingObject ?? _parentServiceProvider.GetService(serviceType);
            }
        }
    }
}