using System.Reflection;
using ViewModels.Interfaces;

namespace ViewModels.Utils;

public class ReflectionUtil
{
    public List<string> AssemblyNames { get; set; } = [];

    private IEnumerable<Assembly> GetAppAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetName().Name is { } s && AssemblyNames.Contains(s));
    }

    public IEnumerable<Type> GetRefreshableViews(Type target)
    {
        var res = GetAppAssemblies()
            .SelectMany(x => x.GetTypes())
            .SelectMany(x =>
            {
                var interfaces = x.GetInterfaces();
                if (interfaces.FirstOrDefault(interface1 => interface1.Name == typeof(IRefreshableView<>).Name) is not
                    { } interfaceType)
                {
                    return [];
                }

                var genericType = interfaceType.GetGenericArguments()[0];
                return genericType == target ? new[] { x } : [];
            });
        return res;
    }
}
