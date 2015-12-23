using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Util
{
    public abstract class LoggingContext
    {
        public static LoggingContext Current { get; set; } = new NullLoggingContext();

        public ILoggerFactory LoggerFactory { get; protected set; }
    }
}