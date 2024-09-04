using FluentAssertions;
using Lib.Utils;
using ViewModels.Config;
using ViewModels.Domain;

namespace ViewModelTests;

public class AssemblyTest
{
    private ServiceCollection _services;
    private ViewModelConfig _clientConfig;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
        var assemblyService = new AssemblyService(AppDomain.CurrentDomain);

        _clientConfig = new ViewModelConfig(assemblyService, _services);
    }

    [Test]
    public void Config_ShouldRetrieveConcreteClassesFromDomainNamespace()
    {
        _clientConfig.AddServices();

        _services
            .Should()
            .ContainSingle(x => x.ServiceType == typeof(InstructorFormViewModelFactory))
            .And.ContainSingle(x => x.ImplementationType == typeof(EditAssessmentViewModel))
            .And.ContainSingle(x => x.ServiceType == typeof(IEditAssessmentViewModel));
    }
}