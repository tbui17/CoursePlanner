using FluentAssertions;
using Lib;
using Lib.Services;
using Lib.Utils;
using ViewModels.Domain;

namespace ViewModelTests;

public class ReflectionTest
{
    [Test]
    public void METHOD()
    {
        var services = new ServiceCollection();
        var assemblyService = new AssemblyService(AppDomain.CurrentDomain);

        var ns = NamespaceData.FromNameofExpression(nameof(Lib.Services));
        assemblyService.GetConcreteClassesInNamespace(ns).Should().ContainSingle(x => x == typeof(AccountService));
        var conf = new BackendConfig(assemblyService, services);
        conf.AddBackendServices();

        services.Where(x => x.ServiceType == typeof(AccountService)).Should().ContainSingle();
    }


}