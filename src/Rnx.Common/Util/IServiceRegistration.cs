using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Util
{
    public interface IServiceRegistration
    {
        void RegisterServices(IServiceCollection services);
        void ConfigureServices(IServiceProvider serviceProvider);
    }
}
