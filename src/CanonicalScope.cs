namespace CanonicalLogging;

public class CanonicalScope(Action onDispose, IDisposable? originalScope = null) : IDisposable
{
    public void Dispose()
    {
        onDispose();
        originalScope?.Dispose();
    }
}