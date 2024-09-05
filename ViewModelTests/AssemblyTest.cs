using FluentAssertions;
using Lib.Attributes;
using Lib.Utils;
using Lib.Validators;
using ViewModels.Config;
using ViewModels.Domain;

namespace ViewModelTests;

public class AssemblyTest
{
    [Test]
    public void Config_ShouldRetrieveConcreteClassesFromDomainNamespace()
    {
        var assemblyService = new AssemblyService(AppDomain.CurrentDomain);
        var services = new ServiceCollection();

        var clientConfig = new ViewModelConfig(assemblyService, services);
        clientConfig.AddServices().AddInjectables();

        services
            .Should()
            .ContainSingle(x => x.ServiceType == typeof(InstructorFormViewModelFactory))
            .And.ContainSingle(x => x.ImplementationType == typeof(EditAssessmentViewModel))
            .And.ContainSingle(x => x.ServiceType == typeof(IEditAssessmentViewModel))
            .And.Subject.Where(x => x.ImplementationType == typeof(LoginFieldValidator))
            .Should()
            .HaveCount(2);
    }
}