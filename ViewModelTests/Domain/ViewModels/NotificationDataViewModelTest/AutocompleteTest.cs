using FluentAssertions;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class AutocompleteTest : BaseTest
{
    [Test]
    public void Types_ContainsNotificationItems()
    {
        var model = Resolve<NotificationDataViewModel>();

        model.Types
            .Should()
            .HaveCountGreaterThan(1)
            .And.Contain("Course")
            .And.Contain(x => x.Contains("Assessment"));
    }
}