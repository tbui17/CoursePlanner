using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace BuildLib.Utils;

[AttributeUsage(AttributeTargets.Class)]
public class InjectAttribute(Type? interfaceType = null, ServiceLifetime lifetime = ServiceLifetime.Transient)
    : Attribute
{
    public ServiceLifetime Lifetime { get; set; } = lifetime;
    public Type? Interface { get; set; } = interfaceType;
}

public static class InjectAttributeExtensions
{
    public static IServiceCollection AddInjectables(
        this IServiceCollection services,
        bool registerAsInterfaceOnly = true
    )
    {
        var serviceDescriptors = GetTypesWithInjectAttribute()
            .SelectMany(x => CreateServiceDescriptors(x.Type, x.Attribute))
            .ToArray();

        var logger = Log.ForContext<InjectAttribute>();
        logger.Information("Adding injectables");


        logger.Debug("Service descriptors: {Count} {Data}", serviceDescriptors.Length, serviceDescriptors);

        foreach (var serviceDescriptor in serviceDescriptors)
        {
            services.Add(serviceDescriptor);
        }


        return services;

        IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                using var _ = LogContext.PushProperty("Assembly", assembly.FullName);
                using var __ = LogContext.PushProperty("Domain", AppDomain.CurrentDomain.FriendlyName);
                var log = Log.ForContext<InjectAttribute>();

                foreach (var loaderException in e.LoaderExceptions)
                {
                    log.Warning(
                        loaderException,
                        "Error during type loading"
                    );
                }

                log.Warning(e, "Returning non-null types from those that were loaded. Some data may be missing.");

                return e.Types.WhereNotNull();
            }
        }

        List<ServiceDescriptor> CreateServiceDescriptors(Type type, InjectAttribute attribute)
        {
            List<ServiceDescriptor> result = [];
            var baseDescriptor = new ServiceDescriptor(type, type, attribute.Lifetime);
            if (attribute.Interface is not { } interfaceType)
            {
                result.Add(baseDescriptor);
                return result;
            }

            var withInterface = new ServiceDescriptor(interfaceType, type, attribute.Lifetime);
            result.Add(withInterface);

            if (!registerAsInterfaceOnly)
            {
                result.Add(baseDescriptor);
            }

            return result;
        }

        ParallelQuery<(Type Type, InjectAttribute Attribute)> GetTypesWithInjectAttribute()
        {
            return AppDomain
                .CurrentDomain.GetAssemblies()
                .AsParallel()
                .SelectMany(GetAssemblyTypes)
                .SelectMany(x =>
                    x.GetCustomAttribute<InjectAttribute>() is { } a ? new[] { (Type: x, Attribute: a) } : []
                );
        }
    }
}