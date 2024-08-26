using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Utils;

public static class Util
{
    public static string Full(string _, [CallerArgumentExpression(nameof(_))] string fullPath = default!)
    {
        return fullPath.Substring(fullPath.IndexOf('(') + 1, fullPath.IndexOf(')') - fullPath.IndexOf('(') - 1);
    }
}

public static class ContainerExtensions
{
    public static T CreateInstance<T>(this IServiceProvider provider)
    {
        return ActivatorUtilities.CreateInstance<T>(provider);
    }
}