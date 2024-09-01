using FluentValidation;
using Lib.Services;
using Lib.Services.MultiDbContext;
using Lib.Services.NotificationService;
using Lib.Services.ReportService;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public static class Configs
{
    public static IServiceCollection AddBackendServices(this IServiceCollection b)
    {
        b
            .AddTransient<NotificationService>()
            .AddTransient<ICourseService, CourseService>()
            .AddTransient<AccountService>()
            .AddTransient<MultiLocalDbContextFactory>()
            .AddTransient<ReportService>()
            .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient)
            .AddMemoryCache(o => { o.TrackStatistics = true; });
        return b;
    }
}