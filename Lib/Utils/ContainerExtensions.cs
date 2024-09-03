using Microsoft.Extensions.DependencyInjection;

namespace Lib.Utils;

public static class ContainerExtensions
{
    public static T CreateInstance<T>(this IServiceProvider provider)
    {
        return ActivatorUtilities.CreateInstance<T>(provider);
    }
}