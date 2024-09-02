using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Utils;

public static class Util
{
    // ReSharper disable once EntityNameCapturedOnly.Global
    public static string NameOf(string value, [CallerArgumentExpression(nameof(value))] string fullPath = "") =>
        fullPath.Substring(7, fullPath.Length - 8);
}

public static class ContainerExtensions
{
    public static T CreateInstance<T>(this IServiceProvider provider)
    {
        return ActivatorUtilities.CreateInstance<T>(provider);
    }
}