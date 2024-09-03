namespace Lib.Utils;

public static class ReflectionExtensions
{
    private static ParallelQuery<Type> GetTypesInSameAssembly<T>(this AppDomain currentDomain)
    {
        var fullName = typeof(T).Assembly.FullName;

        return currentDomain
            .GetAssemblies()
            .AsParallel()
            .Where(x => x.FullName == fullName)
            .SelectMany(x => x.ExportedTypes);
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
            .Where(x => x.Namespace?.StartsWith(ns) ?? false);
    }

    public static IEnumerable<Type> GetConcreteClassesInSameNameSpace<T>(this AppDomain currentDomain)
    {
        return currentDomain
            .GetTypesInSameNamespace<T>()
            .Where(x => x.IsClass)
            .Where(x => !x.IsAbstract);
    }
}