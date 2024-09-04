using FluentValidation;
using Lib.Attributes;
using Lib.Services;
using Lib.Utils;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable RedundantNameQualifier

namespace Lib.Config;

public class BackendConfig(AssemblyService assemblyService, IServiceCollection services)
{
    public IServiceCollection AddServices()
    {
        var data = NamespaceData.FromNameofExpression(nameof(Lib.Services));
        // new ServiceConfigUtil(assemblyService, services).AddClassesAndServices(data);

        services
            .AddInjectables(AppDomain.CurrentDomain)
            .AddTransient<ICourseService, CourseService>()
            .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient);
        return services;
    }
}