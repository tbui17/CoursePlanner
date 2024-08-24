using FluentValidation;
using Lib.Services;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public class Configs
{
    public static IServiceCollection ConfigBackendServices(IServiceCollection b)
    {
        b
           .AddTransient<NotificationService>()
           .AddTransient<ICourseService, CourseService>()
           .AddTransient<AccountService>()
           .AddSingleton<ISessionService, SessionService>()
           .AddKeyedTransient<ISessionService, SessionService>("Transient")
           .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient);
        return b;
    }
}