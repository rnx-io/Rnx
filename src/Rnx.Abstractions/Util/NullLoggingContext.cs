using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Util
{
    public class NullLoggingContext : LoggingContext
    {
        public NullLoggingContext()
        {
            LoggerFactory = new NullLoggerFactory();
        }

        private class NullLoggerFactory : ILoggerFactory
        {
            public LogLevel MinimumLevel { get; set; }

            public void AddProvider(ILoggerProvider provider)
            { }

            public ILogger CreateLogger(string categoryName)
            {
                return new NullLogger();
            }

            public void Dispose()
            { }

            private class NullLogger : ILogger
            {
                public IDisposable BeginScopeImpl(object state)
                {
                    return null;
                }

                public bool IsEnabled(LogLevel logLevel)
                {
                    return false;
                }

                public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
                { }
            }
        }
    }
}
