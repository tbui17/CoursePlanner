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


    [Test, Ignore("Not implemented")]
    public async Task NotificationItems_PageChangedBy1_ChangesToNextSet()
    {
        var fixture = CreateFixture();

        var (data, expected) = CreateNotificationData(fixture);

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

        await model.WhenAnyValue(x => x.NotificationItems)
            .WhereNotNull()
            .Take(1)
            .ToTask();

        var task = model.WhenAnyValue(x => x.NotificationItems)
            .WhereNotNull()
            .Where(x => x.Count is 10)
            .Where(items => items.IntersectBy(data.Select(x => x.Id), x => x.Id).Any())
            .ToTask();

        model.ChangePageCommand.Execute(2);
        await task;
        model.NotificationItems
            .Should()
            .BeEquivalentTo(expected);
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



        await model.WhenAnyValue(x => x.NotificationItems).WhereNotNull().FirstAsync();
        model.Pages.Should().BeGreaterThan(1);
    }
}