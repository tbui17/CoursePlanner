using FluentValidation;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;


namespace Lib.Config;

public static class BackendConfig
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient)
            .AddTransient<AppDomain>(_ => AppDomain.CurrentDomain)
            .AddMemoryCache();

        return services;
    }
}