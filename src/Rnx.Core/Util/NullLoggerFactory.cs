using Microsoft.Extensions.Logging;
using System;

namespace Rnx.Core.Util
{
    public class NullLoggerFactory : ILoggerFactory
    {
        private static readonly ILogger _nullLogger = new NullLogger();

        public LogLevel MinimumLevel { get; set; }

        public void AddProvider(ILoggerProvider provider)
        { }

        public ILogger CreateLogger(string categoryName) => _nullLogger;

        public void Dispose()
        {}

        private class NullLogger : ILogger
        {
            public IDisposable BeginScopeImpl(object state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel) => false;

            public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter) { }
        }
    }
}
