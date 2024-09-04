using FluentValidation;
using Lib.Services;
using Lib.Utils;
using Lib.Validators;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable RedundantNameQualifier

namespace Lib.Config;

public class BackendConfig(AssemblyService assemblyService, IServiceCollection services) : ServiceConfigurator(assemblyService, services)
{
    private readonly IServiceCollection _services = services;

    public IServiceCollection AddServices()
    {
        var data = NamespaceData.FromNameofExpression(nameof(Lib.Services));
        AddClassesAndServices(data);
        _services
            .AddTransient<ICourseService, CourseService>()
            .AddValidatorsFromAssemblyContaining<LoginFieldValidator>(ServiceLifetime.Transient);
        return _services;
    }
}