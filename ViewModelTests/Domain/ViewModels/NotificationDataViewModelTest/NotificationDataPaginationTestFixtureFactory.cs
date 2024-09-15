using AutoFixture;
using BaseTestSetup.Lib;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class NotificationDataPaginationTestFixtureDataFactory(IFixture fixture, IServiceProvider provider)
{

    public List<INotification> CreateNotificationData()
    {
        var data = fixture.CreateMany<NotificationData>(50)
            .Select((x, i) =>
            {
                x.Id = i + 1;
                x.Name = $"Notification {x.Id}";
                return x;
            })
            .Cast<INotification>()
            .ToList();

        return data;
    }

    public (List<INotification> data, Mock<INotificationDataService> dataService) CreateDataServiceWithData()
    {
        var data = CreateNotificationData();
        var dataService = fixture.FreezeMock<INotificationDataService>();
        fixture.Register(provider.GetRequiredService<ILogger<NotificationDataViewModel>>);
        fixture.Register(provider.GetRequiredService<ILogger<NotificationDataStreamService>>);
        fixture.Register(provider.GetRequiredService<INotificationDataViewModelDefaultsProvider>);
        // fixture.Register<ILogger<NotificationDataViewModel>>(() => new FakeLogger<NotificationDataViewModel>());
        // fixture.Register<ILogger<NotificationDataStreamService>>(() => new FakeLogger<NotificationDataStreamService>());

        dataService
            .Setup(x => x.GetNotificationsWithinDateRange(It.IsAny<IDateTimeRange>()))
            .Callback<IDateTimeRange>(x =>
            {
                Log.ForContext<NotificationDataPaginationTestFixture>().Information("GetNotificationsWithinDateRange called with {DateRange}", x);
            })
            .ReturnsAsync(data);

        return (data, dataService);
    }


    public NotificationDataPaginationTestFixture CreateFixture()
    {

        var (data, dataService) = CreateDataServiceWithData();

        var model = new NotificationDataViewModel(
            fixture.Create<NotificationFilterService>(),
            fixture.Create<ILogger<NotificationDataViewModel>>(),
            fixture.Create<INotificationDataViewModelDefaultsProvider>(),
            provider.GetRequiredService<AutocompleteService>()

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