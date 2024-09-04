using Lib.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Config;

public class ServiceConfigUtil(AssemblyService assemblyService, IServiceCollection services)
{
    public IServiceCollection AddClassesAndServices(NamespaceData data)
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