using System.Reflection;
using Lib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace Lib.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class InjectAttribute(Type? interfaceType = null, ServiceLifetime lifetime = ServiceLifetime.Transient)
    : Attribute
{
    public ServiceLifetime Lifetime { get; set; } = lifetime;
    public Type? Interface { get; set; } = interfaceType;
}

public static class InjectAttributeExtensions
{
    public static IServiceCollection AddInjectables(this IServiceCollection services)
    {
        var appDomain = AppDomain.CurrentDomain;
        var types = appDomain.GetAssemblies()
            .AsParallel()
            .SelectMany(GetTypes)
            .SelectMany(x => x.GetCustomAttribute<InjectAttribute>() is { } a ? new[] { (Type: x, Attribute: a) } : [])
            .Select(x => (x.Type, CreateAddImpl(x.Attribute)))
            .ToArray();

        var logger = Log.ForContext<InjectAttribute>();
        logger.Information("Adding injectables");

        foreach (var (classType, addImpl) in types)
        {
            addImpl(classType);
        }

        logger.Debug("{Count} injectables: {Items}", types.Length,types);


        return services;

        IEnumerable<Type> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                using var _ = LogContext.PushProperty("Assembly", assembly.FullName);
                using var __ = LogContext.PushProperty("Domain", appDomain.FriendlyName);
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


        Action<Type> CreateAddImpl(InjectAttribute attribute) => attribute switch
        {
            { Interface: null, Lifetime: ServiceLifetime.Transient } => type => services.AddTransient(type),
            { Interface: null, Lifetime: ServiceLifetime.Scoped } => type => services.AddScoped(type),
            { Interface: null, Lifetime: ServiceLifetime.Singleton } => type => services.AddSingleton(type),
            { Interface: { } interfaceType, Lifetime: ServiceLifetime.Transient } => type =>
                services.AddTransient(interfaceType, type),
            { Interface: { } interfaceType, Lifetime: ServiceLifetime.Scoped } => type =>
                services.AddScoped(interfaceType, type),
            { Interface: { } interfaceType, Lifetime: ServiceLifetime.Singleton } => type =>
                services.AddSingleton(interfaceType, type),
            _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null)
        };
    }
}
