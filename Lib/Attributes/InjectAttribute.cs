using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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
            .SelectMany(x => x.ExportedTypes)
            .SelectMany(x => x.GetCustomAttribute<InjectAttribute>() is { } a ? new[] { (Type: x, Attribute: a) } : [])
            .Select(x => (x.Type, CreateAddImpl(x.Attribute)));

        foreach (var (classType, addImpl) in types)
        {
            addImpl(classType);
        }

        return services;

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