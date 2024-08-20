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
           .AddValidatorsFromAssemblyContaining<LoginFieldValidator>();
        return b;
    }
}