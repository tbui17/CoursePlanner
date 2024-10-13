using BuildLib.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLib.Utils;

public static class ProviderExtensions
{
    public static T? GetConfiguration<T>(this IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().Get<T>();

    public static T? GetConfiguration<T>(this IConfiguration config) =>
        config.Get<T>();

    public static T? GetConfiguration<T>(this IConfiguration config, string name) =>
        config.GetValue<T>(name);

    public static T? GetConfiguration<T>(this IServiceProvider provider, string name) =>
        provider.GetRequiredService<IConfiguration>().GetConfiguration<T>(name);

    public static T? GetAppConfiguration<T>(
        this IServiceProvider provider,
        Func<CoursePlannerConfiguration, T> selector
    ) =>
        provider.GetAppConfiguration() is { } config
            ? selector(config)
            : default;

    public static CoursePlannerConfiguration? GetAppConfiguration(
        this IServiceProvider provider
    ) =>
        provider.GetConfiguration<CoursePlannerConfiguration>();

    public static T GetConfigurationOrThrow<T>(this IServiceProvider provider) =>
        provider.GetConfiguration<T>() ??
        throw new ArgumentException($"{typeof(T)} was null.");

    public static T GetConfigurationOrThrow<T>(this IConfiguration config) =>
        config.GetConfiguration<T>() ??
        throw new ArgumentException($"{typeof(T)} was null.");

    public static T GetConfigurationOrThrow<T>(this IConfiguration config, string name) =>
        config.GetConfiguration<T>(name) ??
        throw new ArgumentException($"{name} {typeof(T)} was null.");

    public static T GetConfigurationOrThrow<T>(this IServiceProvider provider, string name) =>
        provider.GetConfiguration<T>(name) ??
        throw new ArgumentException($"{name} {typeof(T)} was null.");

    public static T GetAppConfigurationOrThrow<T>(
        this IServiceProvider provider,
        Func<CoursePlannerConfiguration, T> selector
    ) =>
        selector(provider.GetAppConfigurationOrThrow());

    public static CoursePlannerConfiguration GetAppConfigurationOrThrow(
        this IServiceProvider provider
    ) =>
        provider.GetConfigurationOrThrow<CoursePlannerConfiguration>();
}