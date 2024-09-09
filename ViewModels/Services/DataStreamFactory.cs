using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Domain;
using ViewModels.Models;

namespace ViewModels.Services;

[Inject]
public class DataStreamFactory(NotificationService notificationService, ILogger<DataStreamFactory> logger)
{
    private static ParallelQuery<INotification> ApplyNotificationFilter(
        ParallelQuery<INotification> results,
        int notificationSelectedIndex
    ) =>
        notificationSelectedIndex switch
        {
            < 1 => results,
            1 => results.Where(item => item.ShouldNotify),
            > 1 => results.Where(item => !item.ShouldNotify)
        };

    public IObservable<List<INotification>> CreateDataStream(InputSource inputSource)
    {

        return inputSource.DateFilterSource
            .ObserveOn(RxApp.TaskpoolScheduler)
            .SelectMany(notificationService.GetNotificationsWithinDateRange)
            .CombineLatest(inputSource.TextFilterSource, inputSource.PickerFilterSource, inputSource.RefreshSource)
            .Select(sources =>
            {
                var (notifications, (filterText, typeFilter), notificationSelectedIndex, _) = sources;

                return notifications
                    .AsParallel()
                    .Thru(notificationStream => ApplyNotificationFilter(notificationStream, notificationSelectedIndex))
                    .Where(item => item.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase))
                    .Where(item =>
                        item is Assessment assessment
                            ? $"{assessment.Type} Assessment".Contains(typeFilter,
                                StringComparison.CurrentCultureIgnoreCase)
                            : item.GetType().Name.Contains(typeFilter, StringComparison.CurrentCultureIgnoreCase));
            })
            .Select(x => x.ToList())
            .Do(x => logger.LogDebug("Notification count {Count}", x.Count))
            .Catch((Exception exception) =>
            {
                logger.LogError(exception, "Error getting notifications: {Exception}",exception);
                return Observable.Return(new List<INotification>());
            });
    }
}