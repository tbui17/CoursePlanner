using BuildLib.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLib.Utils;

public static class ProviderExtensions
{
    public static T GetConfiguration<T>(this IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().Get<T>() ??
        throw new ArgumentException($"{typeof(T)} was null.");

    public static T GetConfiguration<T>(this IConfiguration config) =>
        config.Get<T>() ??
        throw new ArgumentException($"{typeof(T)} was null.");

    public static T GetConfiguration<T>(this IConfiguration config, string name) => config.GetValue<T>(name) ??
        throw new ArgumentException($"{name} {typeof(T)} was null.");

    public static T GetConfiguration<T>(this IServiceProvider provider, string name) =>
        provider.GetRequiredService<IConfiguration>().GetConfiguration<T>(name);

    public static T GetAppConfiguration<T>(
        this IServiceProvider provider,
        Func<CoursePlannerConfiguration, T> selector
    ) =>
        selector(provider.GetAppConfiguration());

    public static CoursePlannerConfiguration GetAppConfiguration(
        this IServiceProvider provider
    ) =>
        provider.GetConfiguration<CoursePlannerConfiguration>();
}