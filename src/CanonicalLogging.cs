using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CanonicalLogging;

public class CanonicalLogger(string categoryName, ILogger originalLogger) : ILogger
{
    private readonly string _categoryName = categoryName;
    private readonly ConcurrentQueue<CanonicalLogEntry> _logEntries = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        var originalScope = originalLogger.BeginScope(state);
        return new CanonicalScope(() => { }, originalScope);
    }

    public bool IsEnabled(LogLevel logLevel) => originalLogger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        originalLogger.Log(logLevel, eventId, state, exception, formatter);

        if (state is IEnumerable<KeyValuePair<string, object>> stateProps)
        {
            var entry = new CanonicalLogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = formatter(state, exception),
                Exception = exception,
                State = stateProps.ToDictionary(x => x.Key, x => (object?)x.Value)
            };
            _logEntries.Enqueue(entry);
        }
    }

    public IEnumerable<CanonicalLogEntry> FlushLogs()
    {
        var entries = new List<CanonicalLogEntry>();
        while (_logEntries.TryDequeue(out var entry))
        {
            entries.Add(entry);
        }
        return entries;
    }
}