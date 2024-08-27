using FluentValidation;
using Lib.Services;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public static class Configs
{
    public static IServiceCollection ConfigBackendServices(IServiceCollection b)
    {
        b
            .AddTransient<NotificationService>()
            .AddTransient<ICourseService, CourseService>()
            .AddTransient<AccountService>()
            .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient)
            .AddMemoryCache(o => { o.TrackStatistics = true; });
        return b;
    }
}