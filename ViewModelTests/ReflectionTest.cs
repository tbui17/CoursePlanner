using FluentAssertions;
using Lib.Utils;
using ViewModels.Domain;

namespace ViewModelTests;

public class ReflectionTest
{

    [Test]
    public void GetClassesInSameNamespace_ShouldNotBeEmpty()
    {
        AppDomain.CurrentDomain.GetClassesInSameNamespace<MainViewModel>().Should().NotBeEmpty();
    }
}