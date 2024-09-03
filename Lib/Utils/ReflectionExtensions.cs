using System.Reflection;

namespace Lib.Utils;

public static class ReflectionExtensions
{
    private static List<Type> GetTypesInSameAssembly<T>(this AppDomain currentDomain)
    {
        var type = typeof(T);
        var fullName = type.Assembly.FullName;
        var assemblies = currentDomain.GetAssemblies();

        return assemblies
            .AsParallel()
            .SelectMany(x => x.ExportedTypes)
            .Where(x => x.Assembly.FullName == fullName)
            .ToList();


        IEnumerable<Type> GetTypes(Assembly x)
        {
            // https://stackoverflow.com/questions/56254916/could-not-load-type-castle-proxies-ireadinessproxy-when-running-xunit-integratio
            // https://github.com/castleproject/Core/issues/193
            const int limit = 3;
            var tryCount = 0;
            while (tryCount < limit)
            {
                try
                {
                    tryCount++;
                    return x.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException e) when (IsProxyError(e))
                {

                }
            }

            throw new ReflectionTypeLoadException([type], []);

            static bool IsProxyError(ReflectionTypeLoadException e)
            {
                const string errorMessage = "Could not load type 'Castle.Proxies";
                return e.Message.Contains(errorMessage);
            }
        }
    }

    private static ParallelQuery<Type> GetTypesInSameNamespace<T>(this AppDomain currentDomain)
    {
        var ns = typeof(T).Namespace;
        if (ns is null)
        {
            return Enumerable.Empty<Type>().AsParallel();
        }

        return currentDomain
            .GetTypesInSameAssembly<T>()
            .AsParallel()
            .Where(x => x.Namespace?.StartsWith(ns) ?? false);
    }

    public static IEnumerable<Type> GetClassesInSameNamespace<T>(this AppDomain currentDomain)
    {
        return currentDomain
            .GetTypesInSameNamespace<T>()
            .Where(x => x.IsClass)
            .Where(x => !x.IsAbstract);
    }
}