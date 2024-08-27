using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace Lib.Services;

public class RefreshableViewService(IMemoryCache cache)
{
    public ICollection<string> AssemblyNames { get; set; } = [];
    public Type RefreshableViewType { get; set; } = typeof(object);

    private IEnumerable<Assembly> GetAppAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetName().Name is { } s && AssemblyNames.Contains(s));
    }

    public IEnumerable<Type> GetRefreshableViewsContainingTarget(Type target)
    {
        if (cache.Get<IEnumerable<Type>>(target) is { } cachedResult)
        {
            return cachedResult;
        }

        var res = GetRefreshableViews()
            .Where(x => x.ViewModelType == target)
            .Select(x => x.PageType)
            .ToList();

        cache.Set(target, res);
        return res;
    }

    public void InitializeCache()
    {
        var groupings = GetRefreshableViews().GroupBy(x => x.ViewModelType);
        foreach (var g in groupings)
        {
            var res = g.Select(x => x.PageType).ToList();
            cache.Set(g.Key, res);
        }
    }

    private IEnumerable<(Type PageType, Type ViewModelType)> GetRefreshableViews() =>
        GetAppAssemblies()
            .SelectMany(x => x.GetTypes())
            .SelectMany(type =>
            {
                if (type.GetInterfaces()
                        .FirstOrDefault(inter => inter.IsAssignableTo(RefreshableViewType))
                    is not { } interfaceType
                   )
                {
                    return [];
                }

                var genericType = interfaceType.GetGenericArguments().Single();
                return new[] { (type, genericType) };
            });
}