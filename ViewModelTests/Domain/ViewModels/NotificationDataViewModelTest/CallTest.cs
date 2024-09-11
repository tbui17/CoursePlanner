using BaseTestSetup.Lib;
using FluentAssertions;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class CallTest : BaseTest
{
    [Test]
    public async Task Initialization_HitsDataServiceOnce()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();

        await f.Model.Should().EventuallySatisfy(x => x.NotificationItems.Should().HaveCountGreaterThan(1));


        f.DataService.ShouldCall(x => nameof(x.GetNotificationsWithinDateRange), 1);


    }
}