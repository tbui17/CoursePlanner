using System.Linq.Expressions;
using BuildLib.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLib.Utils;

public static class ProviderExtensions
{
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
    )
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var appConfig = config.Get<AppConfiguration>();
        ArgumentNullException.ThrowIfNull(appConfig, "AppConfiguration");
        return appConfig;
    }
}