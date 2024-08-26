using System.Reflection;
using ViewModels.Interfaces;

namespace ViewModels.Utils;

public interface IRefreshableViewCache
{
    IReadOnlyCollection<Type>? Get(Type target);
    void Add(Type target, IReadOnlyCollection<Type> views);
}

public class RefreshableViewCache : IRefreshableViewCache
{
    private Dictionary<Type, IReadOnlyCollection<Type>> Cache { get; } = new();

    public IReadOnlyCollection<Type>? Get(Type target)
    {
        return Cache.TryGetValue(target, out var cached) ? cached : null;
    }

    public void Add(Type target, IReadOnlyCollection<Type> views)
    {
        Cache[target] = views;
    }
}

public class ReflectionUtil
{
    public IReadOnlyCollection<string> AssemblyNames { get; set; } = [];

    public IRefreshableViewCache Cache { get; set; } = new RefreshableViewCache();

    private IEnumerable<Assembly> GetAppAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetName().Name is { } s && AssemblyNames.Contains(s));
    }

    public IEnumerable<Type> GetRefreshableViewsContainingTarget(Type target)
    {
        if (Cache.Get(target) is {} cached)
        {
            return cached;
        }

        var query = GetAppAssemblies()
            .SelectMany(x => x.GetTypes())
            .SelectMany(type =>
            {
                var interfaces = type.GetInterfaces();
                if (interfaces.FirstOrDefault(interface1 => interface1.Name == typeof(IRefreshableView<>).Name) is not
                    { } interfaceType)
                {
                    return [];
                }

                var genericType = interfaceType.GetGenericArguments().Single();
                return genericType == target ? new[] { type } : [];
            });

        var res = query.ToList();
        Cache.Add(target, res);
        return res;
    }
}