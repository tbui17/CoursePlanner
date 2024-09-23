using FluentAssertions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;
[NonParallelizable]
public class DateValidationTest : BaseTest
{
    [Test]
    public async Task ChangeStartDate_StartGtEnd_CapsAtEnd()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();

        var today = DateTime.Today.Date;
        var tomorrow = today.AddDays(1);
        f.Model.ChangeStartDate(today);
        f.Model.ChangeEndDate(tomorrow);

        var dayAfter = tomorrow.AddDays(1);
        f.Model.ChangeStartDate(dayAfter);

        await f.Model.Should().EventuallySatisfy(x => x.Start.Should().Be(tomorrow).And.Be(x.End));
    }

    [Test]
    public async Task ChangeEndDate_EndLtStart_CapsAtStart()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();

        var today = new DateTime(year: 2020, month: 1, day: 10);
        var yesterday = new DateTime(year: 2020, month: 1, day: 9);
        var tomorrow = new DateTime(year: 2020, month: 1, day: 11);

        var setRangeToTodayAndTomorrow = () =>
            {
                f.Model.ChangeStartDate(today);
                f.Model.ChangeEndDate(tomorrow);
            };

        var setEndToYesterday = () =>
        {
            f.Model.ChangeEndDate(yesterday);

        };


        setRangeToTodayAndTomorrow();
        setEndToYesterday();

        await f.Model.Should().EventuallySatisfy(x => x.End.Should().Be(today).And.Be(x.End));
    }

    [Test]
    public async Task ChangeStartDate_EqualToOver_CapsAtEnd()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();

        var today = DateTime.Today.Date;
        f.Model.ChangeStartDate(today);
        f.Model.ChangeEndDate(today);

        var dayAfter = today.AddDays(1);
        f.Model.ChangeStartDate(dayAfter);

        await f.Model.Should().EventuallySatisfy(x => x.Start.Should().Be(today).And.Be(x.End));
    }

    [Test]
    public async Task ChangeEndDate_EqualToOver_CapsAtStart()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();

        var today = DateTime.Today.Date;
        f.Model.ChangeStartDate(today);
        f.Model.ChangeEndDate(today);

        var yesterday = today.AddDays(-1);
        f.Model.ChangeEndDate(yesterday);

        await f.Model.Should().EventuallySatisfy(x => x.Start.Should().Be(today).And.Be(x.End));
    }
}