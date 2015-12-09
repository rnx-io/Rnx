using Rnx.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Tasks.Core.FileSystem;
using Rnx.Common.Execution.Decorators;
using Rnx.Tasks.Core.Composite;
using Rnx.Tasks.Core.Control;

namespace Rnx.Tasks.Core
{
    public class ServiceRegistration : IServiceRegistration
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IFileSystem, DefaultFileSystem>();
        }

        public void ConfigureServices(IServiceProvider serviceProvider)
        {
            var nullLoggingDecorator = new NullLoggingExecutionDecorator();
            var decoratorService = serviceProvider.GetService<ITaskDecoratorService>();
            decoratorService.Decorate(typeof(SeriesTask), nullLoggingDecorator)
                            .Decorate(typeof(ParallelTask), nullLoggingDecorator)
                            .Decorate(typeof(IfTask), nullLoggingDecorator)
                            .Decorate(typeof(FilterTask), nullLoggingDecorator);
        }
    }
}