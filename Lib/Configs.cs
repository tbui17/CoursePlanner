using System.Reflection;
using FluentValidation;
using Lib.Models;
using Lib.Services;
using Lib.Services.MultiDbContext;
using Lib.Services.NotificationService;
using Lib.Services.ReportService;
using Lib.Utils;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public static class Configs
{
    public static IServiceCollection AddBackendServices(this IServiceCollection b)
    {
        foreach (var type in AppDomain.CurrentDomain.GetConcreteClassesInSameNameSpace<RootReflectionService>())
        {
            b.AddTransient(type);
        }

        b
            .AddTransient<ICourseService, CourseService>()
            .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient);
        return b;
    }
}