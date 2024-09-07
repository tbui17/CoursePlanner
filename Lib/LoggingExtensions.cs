using Microsoft.Extensions.Logging;

namespace Lib;

public static class LoggingExtensions
{

    public static IDisposable? BeginScope2<T>(this ILogger<T> logger, string name, IReadOnlyDictionary<string,object> values)
    {
        return logger.BeginScope(name, values);

    }
}