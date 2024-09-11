using AutoFixture;
using BaseTestSetup.Lib;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using ViewModels.Domain;
using ViewModels.Services;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class NotificationDataPaginationTestFixtureDataFactory(IFixture fixture)
{
    public List<INotification> CreateNotificationData()
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

        return data;
    }


    public NotificationDataPaginationTestFixture CreateFixture()
    {
        var data = CreateNotificationData();

        var dataService = fixture.FreezeMock<INotificationDataService>();

        dataService
            .Setup(x => x.GetNotificationsWithinDateRange(It.IsAny<IDateTimeRange>()))
            .ReturnsAsync(data);

        var model = new NotificationDataViewModel(
            fixture.Create<NotificationDataStreamFactory>(),
            new FakeLogger<NotificationDataViewModel>()
        );

        return new NotificationDataPaginationTestFixture
        {
            Fixture = fixture,
            Data = data,
            DataService = dataService,
            Model = model,
            Expected = data
        };
    }

    public static NotificationDataPaginationTestFixture CreatePaginationFixture()
    {
        var fixture = Globals.CreateFixture();
        return new NotificationDataPaginationTestFixtureDataFactory(fixture).CreateFixture();
    }
}