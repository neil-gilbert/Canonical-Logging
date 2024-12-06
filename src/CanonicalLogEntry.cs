namespace CanonicalLogging;

using Microsoft.Extensions.Logging;

public class CanonicalLogEntry
{
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public IDictionary<string, object?> State { get; set; } = new Dictionary<string, object?>();
}