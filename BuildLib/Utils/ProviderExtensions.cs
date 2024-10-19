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
        Func<AppConfiguration, T> selector
    ) =>
        provider.GetAppConfiguration() is { } config
            ? selector(config)
            : default;

    public static AppConfiguration? GetAppConfiguration(
        this IServiceProvider provider
    ) =>
        provider.GetConfiguration<AppConfiguration>();

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
        Func<AppConfiguration, T> selector
    )
    {
        var res = selector(provider.GetAppConfigurationOrThrow());
        if (res is string s)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(s);
        }

        ArgumentNullException.ThrowIfNull(res);
        return res;
    }

    public static AppConfiguration GetAppConfigurationOrThrow(
        this IServiceProvider provider
    ) =>
        provider.GetConfigurationOrThrow<AppConfiguration>();
}