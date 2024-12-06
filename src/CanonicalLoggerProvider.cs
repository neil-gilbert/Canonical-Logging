using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CanonicalLogging;

public class CanonicalLoggerProvider(ILoggerFactory originalFactory) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CanonicalLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => 
            new CanonicalLogger(name, originalFactory.CreateLogger(name)));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }

    public IEnumerable<CanonicalLogEntry> FlushAllLogs()
    {
        return _loggers.Values.SelectMany(logger => logger.FlushLogs());
    }
}