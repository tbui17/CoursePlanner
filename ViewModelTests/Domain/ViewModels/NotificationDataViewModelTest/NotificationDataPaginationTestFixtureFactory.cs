using AutoFixture;
using BaseTestSetup.Lib;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Serilog;
using ViewModels.Domain;
using ViewModels.Services;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class NotificationDataPaginationTestFixtureDataFactory(IFixture fixture, IServiceProvider provider)
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
        fixture.Register(provider.GetRequiredService<ILogger<NotificationDataViewModel>>);
        fixture.Register(provider.GetRequiredService<ILogger<NotificationDataStreamFactory>>);
        // fixture.Register<ILogger<NotificationDataViewModel>>(() => new FakeLogger<NotificationDataViewModel>());
        // fixture.Register<ILogger<NotificationDataStreamFactory>>(() => new FakeLogger<NotificationDataStreamFactory>());

        dataService
            .Setup(x => x.GetNotificationsWithinDateRange(It.IsAny<IDateTimeRange>()))
            .Callback<IDateTimeRange>(x =>
            {
                Log.ForContext<NotificationDataPaginationTestFixture>().Information("GetNotificationsWithinDateRange called with {DateRange}", x);
            })
            .ReturnsAsync(data);

        var model = new NotificationDataViewModel(
            fixture.Create<NotificationDataStreamFactory>(),
            fixture.Create<ILogger<NotificationDataViewModel>>()


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
}