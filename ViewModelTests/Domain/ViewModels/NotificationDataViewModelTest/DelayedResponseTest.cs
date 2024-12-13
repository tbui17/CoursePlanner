using System.Reactive.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
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
        var stub = new DataServiceStub
        {
            Response1 = response1,
            Response2 = response2
        };

        var model = new NotificationDataViewModel(
            notificationFilterService: new NotificationFilterService(
                service: new NotificationDataStreamService(
                    notificationDataService: stub,
                    logger: Resolve<ILogger<NotificationDataStreamService>>(),
                    completeInputModelFactory: Resolve<PageResultFactory>()
                )
            ),
            logger: Resolve<ILogger<NotificationDataViewModel>>(),
            defaultsProvider: Resolve<INotificationDataViewModelDefaultsProvider>(),
            autocompleteService: Resolve<AutocompleteService>()
        );

        // initial load
        await stub.ShouldEventuallySatisfy(x => x.ModelReceivedOneResponse.Should().BeTrue());

        var pageResult = model
            .WhenAnyValue(x => x.PageResult);

        // listen for response1
        var response1Data = pageResult
            .Where(x => x.CurrentPageData.Contains(response1))
            .FirstAsync();

        // model should never have response1 in its data
        response1Data.Subscribe(_ => throw new AssertionException("Response1 should not be in the data"));

        // listen for response2
        var response2Data = pageResult
            .Where(x => x.CurrentPageData.Contains(response2))
            .FirstAsync();

        await SetEndDateTwice();

        // wait for response2 to be exposed in PageResult
        await response2Data;

        // wait for stub to return response1
        await stub.ShouldEventuallySatisfy(x => x.DidSetResponse1.Should().BeTrue());

        // give 2 seconds for model to receive data from stub, it should reject the data and never expose it in PageResult
        await response1Data
            .Invoking(async x => await x.Timeout(2.Seconds()))
            .Should()
            .ThrowWithinAsync<TimeoutException>(3.Seconds());

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
        var stub = new DataServiceStub
        {
            Response1 = response1,
            Response2 = new()
        };

        var model = new NotificationDataViewModel(
            new NotificationFilterService(
                new NotificationDataStreamService(
                    notificationDataService: stub,
                    logger: Resolve<ILogger<NotificationDataStreamService>>(),
                    completeInputModelFactory: Resolve<PageResultFactory>()
                )
            ),
            logger: Resolve<ILogger<NotificationDataViewModel>>(),
            defaultsProvider: Resolve<INotificationDataViewModelDefaultsProvider>(),
            autocompleteService: Resolve<AutocompleteService>()
        );

        // initial load
        await stub.ShouldEventuallySatisfy(x => x.ModelReceivedOneResponse.Should().BeTrue());
        var date1 = new DateTime(2030, 1, 1);
        model.End = date1;

        await model.Should().EventuallySatisfy(x => x.PageResult.CurrentPageData.Should().Contain(response1));
    }
}

file class DataServiceStub : INotificationDataService
{
    public int Count { get; set; }
    public required Course Response1 { get; set; }
    public required Assessment Response2 { get; set; }
    public bool DidSetResponse1 { get; set; }

    public bool ModelReceivedOneResponse => Count == 1;
    public bool ModelHasNotInitialized => Count == 0;

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

        if (ModelReceivedOneResponse)
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