using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using AutoFixture;
using BaseTestSetup.Lib;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using ReactiveUI;
using ViewModels.Domain;
using ViewModels.Services;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(5000)]
public class NotificationDataPaginationTest : BaseTest
{
    [Test]
    public void TestCreateFixtureData()
    {
        var fixture = CreateFixture();
        var (data, expected) = CreateNotificationData(fixture);
        using var scope = new AssertionScope();
        data.Should()
            .HaveCount(50);
        expected.Should()
            .ContainSingle(x => x.Id == 20)
            .And.ContainSingle(x => x.Id == 11)
            .And.AllSatisfy(x => x.Id.Should().BeInRange(10, 20));
    }

    private static (List<INotification> data, List<INotification> expected) CreateNotificationData(IFixture fixture)
    {
        var data = fixture.CreateMany<NotificationData>(50)
            .Select((x, i) =>
            {
                x.Id = i + 1;
                x.Name = Guid.NewGuid().ToString();
                return x;
            })
            .Cast<INotification>()
            .ToList();

        var expected = data
            .Chunk(10)
            .Skip(1)
            .First()
            .ToList();

        return (data, expected);
    }

    private NotificationDataPaginationTestFixtureData CreateFixtureData()
    {
        var fixture = CreateFixture();

        var data = CreateNotificationData(fixture);

        var dataService = fixture.FreezeMock<INotificationDataService>();

        dataService
            .Setup(x => x.GetNotificationsWithinDateRange(It.IsAny<IDateTimeRange>()))
            .ReturnsAsync(data.data);

        var model = new NotificationDataViewModel(
            fixture.Create<NotificationDataStreamFactory>(),
            new FakeLogger<NotificationDataViewModel>()
        );

        return new NotificationDataPaginationTestFixtureData
        {
            Fixture = fixture,
            Data = data.data,
            DataService = dataService,
            Model = model,
            Expected = data.expected
        };
    }


    [Test]
    public async Task NotificationItems_PageChangedBy1_ChangesToNextSet()
    {
        var fixture = CreateFixture();

        var (data, expected) = CreateNotificationData(fixture);

        var dataIds = data.Select(x => x.Id).ToList();

        var dataService = fixture.FreezeMock<INotificationDataService>();

        dataService.Setup(x => x.GetNotificationsWithinDateRange(It.IsAny<IDateTimeRange>()))
            .ReturnsAsync(data);

        var model = new NotificationDataViewModel(
            fixture.Create<NotificationDataStreamFactory>(),
            new FakeLogger<NotificationDataViewModel>()
        )
        {
            Start = DateTime.Now.Date
        };

        model.ChangePageCommand.Execute(2);

        await model.Should()
            .EventuallySatisfy(x => x.NotificationItems.Should()
                .HaveCount(10)
                .And.AllSatisfy(item => dataIds.Should().Contain(item.Id))
            );
    }

    [Test, Timeout(2000)]
    public async Task Pages_PartitionedBy10_GreaterThan1()
    {
        var fixture = CreateFixture();

        var (data, _) = CreateNotificationData(fixture);

        var dataService = fixture.FreezeMock<INotificationDataService>();

        dataService.Setup(x => x.GetNotificationsWithinDateRange(It.IsAny<IDateTimeRange>()))
            .ReturnsAsync(data);

        var model = new NotificationDataViewModel(
            fixture.Create<NotificationDataStreamFactory>(),
            new FakeLogger<NotificationDataViewModel>()
        )
        {
            Start = DateTime.Now.Date
        };

        await model.Should()
            .EventuallySatisfy(x => x.Pages.Should().BeGreaterThan(1));
    }

    [Test, Timeout(2000), Ignore("WIP")]
    public async Task ChangePage_AboveLimit_DefaultsToMax()
    {
        var f = CreateFixtureData();

        await f.Model.WhenAnyValue(x => x.NotificationItems)
            .WhereNotNull()
            .FirstAsync()
            .ToTask();


        f.Model.ChangePageCommand.Execute(1000);

        await f.Model.Should().EventuallySatisfy(x => x.CurrentPage.Should().Be(5));
    }
}