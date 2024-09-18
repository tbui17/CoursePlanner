using Lib.Attributes;
using Lib.Interfaces;
using Lib.Providers;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface INotificationDataViewModelDefaultsProvider : IDefaultDateProvider, IDefaultPageProvider;

[Inject(typeof(INotificationDataViewModelDefaultsProvider))]
public class NotificationDataViewModelDefaultsProvider : INotificationDataViewModelDefaultsProvider
{

    private readonly IDefaultDateProvider _defaultDateProvider = new DefaultDateProvider();
    private readonly IDefaultPageProvider _defaultPageProvider = new DefaultPageProvider();

    public IDateTimeRange DateRange => _defaultDateProvider.DateRange;

    public int PageSize => _defaultPageProvider.PageSize;
}