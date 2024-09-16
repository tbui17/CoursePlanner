using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Lib;

public static class LoggingExtensions
{

    public static IDisposable? MethodScope<T>(this ILogger<T> logger, [CallerMemberName] string? methodName = null)
    {
        return logger.BeginScope("{Method}", methodName);

    }
}