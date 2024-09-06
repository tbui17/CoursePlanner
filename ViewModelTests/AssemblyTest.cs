using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Attributes;
using Lib.Services;
using Lib.Validators;
using ViewModels.Config;
using ViewModels.Domain;

namespace ViewModelTests;

public class AssemblyTest
{
    [Test]
    public void Config_ShouldRetrieveConcreteClassesFromDomainNamespace()
    {
        var services = new ServiceCollection();

        var clientConfig = new ViewModelConfig(services);
        clientConfig.AddServices().AddInjectables();

        services
            .Should()
            .ContainSingle(x => x.ServiceType == typeof(InstructorFormViewModelFactory))
            .And.ContainSingle(x => x.ImplementationType == typeof(EditAssessmentViewModel))
            .And.ContainSingle(x => x.ServiceType == typeof(IEditAssessmentViewModel));
    }

    [Test]
    public void Config_ShouldInjectBackendServices()
    {
        var services = new ServiceCollection();

        new ViewModelConfig(services).AddServices().AddInjectables();
        using var _ = new AssertionScope();
        services
            .Where(x => x.ImplementationType == typeof(LoginFieldValidator))
            .Should()
            .HaveCount(2);

        services.Should().ContainSingle(x => x.ServiceType == typeof(IAccountService));
    }
}