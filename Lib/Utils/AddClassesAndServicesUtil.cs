using Microsoft.Extensions.DependencyInjection;

namespace Lib.Utils;

public static class AddClassesAndServicesUtil
{
    public static IServiceCollection AddClassesAndServices(this IServiceCollection services,
        AssemblyService assemblyService, NamespaceData data)
    {
        var classes = assemblyService.GetConcreteClassesInNamespace(data);

        var classesAndInterfaces = classes
            .Select(klass =>
            {
                var iClassName = $"I{klass.Name}";

                var iKlass = klass
                    .GetInterfaces()
                    .SingleOrDefault(x => x.Name == iClassName);

                return (klass, iKlass);
            });

        foreach (var (klass, iKlass) in classesAndInterfaces)
        {
            if (iKlass is not null)
            {
                services.AddTransient(iKlass, klass);
                continue;
            }

            services.AddTransient(klass);
        }

        return services;
    }
}