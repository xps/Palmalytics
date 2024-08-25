using Microsoft.Extensions.Logging;

namespace Palmalytics.Tests.TestHelpers
{
    public class MemoryLogger
    {
        public static List<LogEntry> Logs = new();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }

        public class LogEntry
        {
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; }
            public Exception Exception { get; set; }

            public LogEntry(LogLevel level, string message, Exception exception)
            {
                LogLevel = level;
                Message = message;
                Exception = exception;
            }
        }
    }

    public class MemoryLogger<T> : MemoryLogger, ILogger<T>
    {
    }
}
