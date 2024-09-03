using FluentValidation;
using Lib.Services;
using Lib.Utils;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable RedundantNameQualifier

namespace Lib;

public class BackendConfig(AssemblyService assemblyService, IServiceCollection services)
{
    public IServiceCollection AddBackendServices()
    {
        var res = NamespaceData.FromNameofExpression(nameof(Lib.Services))
            .Thru(assemblyService.GetConcreteClassesInNamespace);


        foreach (var type in res)
        {
            services.AddTransient(type);
        }

        services
            .AddTransient<ICourseService, CourseService>()
            .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient);
        return services;
    }
}