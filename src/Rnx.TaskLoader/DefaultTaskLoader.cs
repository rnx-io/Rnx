using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Rnx.TaskLoader
{
    public class DefaultTaskLoader : ITaskLoader
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultTaskLoader(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<UserDefinedTaskDescriptor> Load(IEnumerable<Type> taskConfigurationTypes, string[] tasksToRun)
        {
            var checkName = new Func<MemberInfo, bool>(f => tasksToRun.Contains(f.Name, StringComparer.OrdinalIgnoreCase));

            foreach (var e in taskConfigurationTypes.Select(f => new { Type = f, Instance = ActivatorUtilities.CreateInstance(_serviceProvider, f) }))
            {
                foreach (var t in e.Type.GetTypeInfo().DeclaredMethods
                                        .Where(f => f.IsPublic && typeof(ITaskDescriptor).IsAssignableFrom(f.ReturnType) && checkName(f))
                                        .Select(f => new UserDefinedTaskDescriptor(f.Name, (ITaskDescriptor)f.Invoke(e.Instance, null))))
                {
                    yield return t;
                }

                foreach (var t in e.Type.GetTypeInfo().DeclaredProperties
                                        .Where(f => typeof(ITaskDescriptor).IsAssignableFrom(f.PropertyType) && f.GetMethod.IsPublic && checkName(f))
                                        .Select(f => new UserDefinedTaskDescriptor(f.Name, (ITaskDescriptor)f.GetGetMethod().Invoke(e.Instance, null))))
                {
                    yield return t;
                }
            }
        }
    }
}