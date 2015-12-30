using Microsoft.Extensions.DependencyInjection;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

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
                foreach (var t in e.Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(f => typeof(ITaskDescriptor).IsAssignableFrom(f.ReturnType) && checkName(f))
                                        .Select(f => new UserDefinedTaskDescriptor(f.Name, (ITaskDescriptor)f.Invoke(e.Instance, null))))
                {
                    yield return t;
                }

                foreach (var t in e.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(f => typeof(ITaskDescriptor).IsAssignableFrom(f.PropertyType) && checkName(f))
                                        .Select(f => {
                                            var taskDescriptor = (ITaskDescriptor)f.GetGetMethod().Invoke(e.Instance, null);
                                            return new UserDefinedTaskDescriptor(f.Name, taskDescriptor);
                                            })
                                        )
                {
                    yield return t;
                }
            }
        }
    }
}