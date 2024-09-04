namespace Lib.Utils;

public class AssemblyService(AppDomain currentDomain)
{
    public ParallelQuery<Type> GetTypesInAssembly(string assemblyName) =>
        currentDomain
            .GetAssemblies()
            .AsParallel()
            .Where(x => x.GetName().Name == assemblyName)
            .SelectMany(x => x.ExportedTypes);


    public ParallelQuery<Type> GetTypesInNamespace(NamespaceData data) =>
        GetTypesInAssembly(data.AssemblyName)
            .Where(x => x.Namespace?.StartsWith(data.FullNamespace) ?? false);


    public IEnumerable<Type> GetConcreteClassesInNamespace(NamespaceData data) =>
        GetTypesInNamespace(data)
            .Where(x => x.IsClass)
            .Where(x => !x.IsAbstract);
}