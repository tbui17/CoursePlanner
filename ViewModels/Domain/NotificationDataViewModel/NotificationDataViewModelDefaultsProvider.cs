using Lib.Attributes;
using Lib.Interfaces;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface INotificationDataViewModelDefaultsProvider : IDefaultDateProvider, IDefaultPageProvider;

[Inject(typeof(INotificationDataViewModelDefaultsProvider))]
public class NotificationDataViewModelDefaultsProvider : INotificationDataViewModelDefaultsProvider
{

    private readonly DefaultDateProvider _defaultDateProvider = new();
    private readonly DefaultPageProvider _defaultPageProvider = new();

    public IDateTimeRange DateRange => _defaultDateProvider.DateRange;

    public int PageSize => _defaultPageProvider.PageSize;
}