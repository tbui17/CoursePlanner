using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface INotificationDataViewModelDefaultsProvider : IDefaultDateProvider, IDefaultPageProvider;

[Inject(typeof(INotificationDataViewModelDefaultsProvider))]
public class NotificationDataViewModelDefaultsProvider : INotificationDataViewModelDefaultsProvider
{
    public IDateTimeRange DateRange
    {
        get
        {
            var now = DateTime.Now.Date;
            return new DateTimeRange
            {
                Start = now,
                End = now.AddMonths(1)
            };
        }
    }

    public int PageSize => 10;
}