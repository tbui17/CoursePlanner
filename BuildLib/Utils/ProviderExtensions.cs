using System.Linq.Expressions;
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
        Expression<Func<AppConfiguration, T>> selector
    )
    {
        var func = selector.Compile();
        T res;
        try
        {
            res = func(provider.GetAppConfigurationOrThrow());
        }
        catch (NullReferenceException e)
        {
            throw new NullReferenceException(
                $"A null reference exception occurred while accessing the value using the selector: {selector}.",
                e
            );
        }


        if (res is string s)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(s, selector.ToString());
        }

        ArgumentNullException.ThrowIfNull(res, selector.ToString());

        return res;
    }

    public static AppConfiguration GetAppConfigurationOrThrow(
        this IServiceProvider provider
    ) =>
        provider.GetConfigurationOrThrow<AppConfiguration>();
}