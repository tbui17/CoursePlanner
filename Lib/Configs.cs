using FluentValidation;
using Lib.Interfaces;
using Lib.Services;
using Lib.Services.MultiDbContext;
using Lib.Services.NotificationService;
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
            .AddTransient<MultiLocalDbContextFactory>()
            .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient)
            .AddMemoryCache(o => { o.TrackStatistics = true; });
        return b;
    }
}