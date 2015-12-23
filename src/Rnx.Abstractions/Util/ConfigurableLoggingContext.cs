using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Util
{
    public class ConfigurableLoggingContext : LoggingContext
    {
        public ConfigurableLoggingContext(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }
    }
}