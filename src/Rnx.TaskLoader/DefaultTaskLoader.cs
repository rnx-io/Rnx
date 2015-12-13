using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rnx.TaskLoader
{
    public class DefaultTaskLoader : ITaskLoader
    {
        private IServiceProvider _serviceProvider;

        public DefaultTaskLoader(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<UserDefinedTask> Load(IEnumerable<Type> taskConfigurationTypes, string[] tasksToRun)
        {
            var checkName = new Func<MemberInfo, bool>(f => tasksToRun.Contains(f.Name, StringComparer.OrdinalIgnoreCase));

            foreach (var e in taskConfigurationTypes.Select(f => new { Type = f, Instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, f) }))
            {
                foreach (var t in e.Type.GetTypeInfo().DeclaredMethods
                                        .Where(f => typeof(ITask).IsAssignableFrom(f.ReturnType) && checkName(f))
                                        .Select(f => new UserDefinedTask(f.Name, (ITask)f.Invoke(e.Instance, null))))
                {
                    yield return t;
                }

                foreach (var t in e.Type.GetTypeInfo().DeclaredProperties
                                        .Where(f => typeof(ITask).IsAssignableFrom(f.PropertyType) && checkName(f))
                                        .Select(f => new UserDefinedTask(f.Name, (ITask)f.GetGetMethod().Invoke(e.Instance, null))))
                {
                    yield return t;
                }
            }
        }
    }
}