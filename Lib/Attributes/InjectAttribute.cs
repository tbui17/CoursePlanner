using System.Reflection;
using System.Text.Json;
using FluentResults;
using Lib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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
    public static IServiceCollection AddInjectables(this IServiceCollection services, AppDomain appDomain)
    {
        var types = appDomain.GetAssemblies()
            .AsParallel()
            .SelectMany(GetTypes)
            .SelectMany(x => x.GetCustomAttribute<InjectAttribute>() is { } a ? new[] { (Type: x, Attribute: a) } : [])
            .Select(x => (x.Type, CreateAddImpl(x.Attribute)));

        foreach (var (classType, addImpl) in types)
        {
            addImpl(classType);
        }

        return services;

        IEnumerable<Type> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var loaderException in e.LoaderExceptions)
                {
                    Log.Error(
                        loaderException,
                        "Error during type loading for {Type} {Method}",
                        nameof(InjectAttributeExtensions),
                        nameof(AddInjectables)
                    );
                }

                Log.Warning("Error during type loading. Returning non-null types from those that were loaded. Some data may be missing.");

                return e.Types.WhereNotNull();
            }
        }

        // @formatter:off
        Action<Type> CreateAddImpl(InjectAttribute attribute) => attribute switch
            {
                { Interface: null, Lifetime: ServiceLifetime.Transient } => type => services.AddTransient(type),
                { Interface: null, Lifetime: ServiceLifetime.Scoped } => type => services.AddScoped(type),
                { Interface: null, Lifetime: ServiceLifetime.Singleton } => type => services.AddSingleton(type),
                { Interface: {} interfaceType, Lifetime: ServiceLifetime.Transient } => type => services.AddTransient(interfaceType, type),
                { Interface: {} interfaceType, Lifetime: ServiceLifetime.Scoped } => type => services.AddScoped(interfaceType, type),
                { Interface: {} interfaceType, Lifetime: ServiceLifetime.Singleton } => type => services.AddSingleton(interfaceType, type),
                _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null)
            };
    }
}