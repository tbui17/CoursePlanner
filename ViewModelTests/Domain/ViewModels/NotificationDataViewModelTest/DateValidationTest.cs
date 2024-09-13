using FluentAssertions;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class DateValidationTest : BaseTest
{
    [Test]
    public async Task Dates_StartGtEnd_CapsAtEnd()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();

        var today = DateTime.Today.Date;
        var tomorrow = today.AddDays(1);
        f.Model.ChangeStartDateCommand.Execute(today);
        f.Model.ChangeEndDateCommand.Execute(tomorrow);

        var dayAfter = tomorrow.AddDays(1);
        f.Model.ChangeStartDateCommand.Execute(dayAfter);

        await f.Model.Should().EventuallySatisfy(x => x.Start.Should().Be(tomorrow).And.Be(f.Model.End));
    }

    [Test]
    public async Task Dates_EndLtStart_CapsAtStart()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();

        var today = new DateTime(year: 2020, month: 1, day: 10);
        var yesterday = new DateTime(year: 2020, month: 1, day: 9);
        var tomorrow = new DateTime(year: 2020, month: 1, day: 11);

        var setRangeToTodayAndTomorrow = () =>
            {
                f.Model.ChangeStartDateCommand.Execute(today);
                f.Model.ChangeEndDateCommand.Execute(tomorrow);
            };

        var setEndToYesterday = () =>
        {
            f.Model.ChangeEndDateCommand.Execute(yesterday);

        };


        setRangeToTodayAndTomorrow();
        setEndToYesterday();

        await f.Model.Should().EventuallySatisfy(x => x.End.Should().Be(today).And.Be(f.Model.Start));
    }
}