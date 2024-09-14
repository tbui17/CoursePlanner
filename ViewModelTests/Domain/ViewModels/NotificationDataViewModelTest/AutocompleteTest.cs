using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class AutocompleteTest : BaseTest
{
    [Test]
    public void Types_ContainsCourseNotificationItem()
    {
        var model = Resolve<NotificationDataViewModel>();


        model.Types
            .Should()
            .NotBeEmpty()
            .And.Contain(nameof(Course));
    }

    private static List<string> GetEntities()
    {
        return DbContextUtil.GetEntityTypes<LocalDbCtx, INotification>().Select(x => x.Name).ToList();
    }

    [Test]
    public void Types_LikelyOnlyContainsEntriesRelatedToDomainModels()
    {
        var model = Resolve<NotificationDataViewModel>();

        var entityNames = GetEntities();

        model.Types
            .Should()
            .HaveCountGreaterThan(1)
            .And.AllSatisfy(typeText => entityNames
                .Should()
                .Contain(entityName => typeText.Contains(entityName, StringComparison.OrdinalIgnoreCase))
            );
    }
}