using System.Reactive.Linq;
using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[NonParallelizable]
[Timeout(10000)]
public class DelayedResponseTest : BaseTest
{
    [SetUp]
    public async override Task Setup()
    {
        await base.Setup();
    }

    [Test]
    [Retry(3)]
    public async Task DateChange_Response1DelayedResponse2Immediate_OnlyReturnsResponseOfLatestRequest()
    {
        var response1 = new Course();
        var response2 = new Assessment();
        var fake = new DataServiceFake
        {
            Response1 = response1,
            Response2 = response2
        };

        var model = new NotificationDataViewModel(
            new NotificationFilterService(
                new NotificationDataStreamService(
                    notificationDataService: fake,
                    logger: Resolve<ILogger<NotificationDataStreamService>>(),
                    completeInputModelFactory: Resolve<PageResultFactory>()
                )
            ),
            logger: Resolve<ILogger<NotificationDataViewModel>>(),
            defaultsProvider: Resolve<INotificationDataViewModelDefaultsProvider>(),
            autocompleteService: Resolve<AutocompleteService>()
        );

        // initial load
        await fake.ShouldEventuallySatisfy(x => x.ModelHasInitialized.Should().BeTrue());

        var pageResult = model
            .WhenAnyValue(x => x.PageResult);

        // model should never have response1 in its data
        pageResult
            .Where(x => x.CurrentPageData.Contains(response1))
            .Subscribe(x => x.CurrentPageData.Should().NotContain(response1));

        // when response2 is set, flag that it happened
        pageResult
            .Where(x => x.CurrentPageData.Contains(response2))
            .Subscribe(_ => fake.DidSetResponse2 = true);

        await SetEndDateTwice();

        await fake.ShouldEventuallySatisfy(x => x.ModelReceivedMultipleRequests.Should().BeTrue());
        await fake.ShouldEventuallySatisfy(x => x.DidSetResponse2.Should().BeTrue());
        await fake.ShouldEventuallySatisfy(x => x.DidSetResponse1.Should().BeTrue());

        await Task.Delay(2000);

        model.PageResult.CurrentPageData.Should().NotContain(response1);

        return;

        async Task SetEndDateTwice()
        {
            var date1 = new DateTime(2030, 1, 1);
            var date2 = new DateTime(2035, 1, 1);
            model.End = date1;
            await Task.Delay(100);
            model.End = date2;
        }
    }

    [Test]
    [Retry(3)]
    public async Task DateChange_Response1Delayed_DoesResolve()
    {
        var response1 = new Course();
        var fake = new DataServiceFake
        {
            Response1 = response1,
            Response2 = new()
        };

        var model = new NotificationDataViewModel(
            new NotificationFilterService(
                new NotificationDataStreamService(
                    notificationDataService: fake,
                    logger: Resolve<ILogger<NotificationDataStreamService>>(),
                    completeInputModelFactory: Resolve<PageResultFactory>()
                )
            ),
            logger: Resolve<ILogger<NotificationDataViewModel>>(),
            defaultsProvider: Resolve<INotificationDataViewModelDefaultsProvider>(),
            autocompleteService: Resolve<AutocompleteService>()
        );

        // initial load
        await fake.ShouldEventuallySatisfy(x => x.ModelHasInitialized.Should().BeTrue());
        var date1 = new DateTime(2030, 1, 1);
        model.End = date1;

        await model.Should().EventuallySatisfy(x => x.PageResult.CurrentPageData.Should().Contain(response1));
    }
}

file class DataServiceFake : INotificationDataService
{
    public int Count { get; set; }
    public required Course Response1 { get; set; }
    public required Assessment Response2 { get; set; }
    public bool DidSetResponse1 { get; set; }
    public bool DidSetResponse2 { get; set; }

    public bool ModelHasInitialized => Count == 1;
    public bool ModelHasNotInitialized => Count == 0;
    public bool ModelReceivedMultipleRequests => Count > 1;

    public Task<IList<INotificationDataResult>> GetUpcomingNotifications(IUserSetting settings)
    {
        throw new NotImplementedException();
    }

    public Task<IList<INotification>> GetNotificationsForMonth(DateTime date)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<INotification>> GetNotificationsWithinDateRange(IDateTimeRange dateRange)
    {
        if (ModelHasNotInitialized)
        {
            Count++;
            return [];
        }

        if (ModelHasInitialized)
        {
            Count++;
            await Task.Delay(3000);
            DidSetResponse1 = true;
            return [Response1];
        }

        Count++;
        return [Response2];
    }

    public Task<int> GetTotalItems()
    {
        throw new NotImplementedException();
    }
}