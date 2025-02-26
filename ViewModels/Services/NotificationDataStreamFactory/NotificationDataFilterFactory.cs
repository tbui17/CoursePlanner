using Lib.Interfaces;
using Lib.Utils;
using ViewModels.Domain.NotificationDataViewModel;

namespace ViewModels.Services.NotificationDataStreamFactory;

public class NotificationDataFilterFactory(IFilterData data)
{
    private static Func<INotification, bool> CreateShouldNotifyFilter(ShouldNotifyIndex index) =>
        index switch
        {
            ShouldNotifyIndex.True => n => n.ShouldNotify,
            ShouldNotifyIndex.False => n => !n.ShouldNotify,
            _ => _ => true
        };


    public IList<Func<INotification, bool>> CreateFilters() =>
    [
        n => n.Name.Contains(data.FilterText, StringComparison.CurrentCultureIgnoreCase),
        n => n.GetFriendlyType().Contains(data.TypeFilter, StringComparison.CurrentCultureIgnoreCase),
        CreateShouldNotifyFilter(data.NotificationSelectedIndex)
    ];

    public Func<INotification, bool> CreateFilter()
    {
        return CreateFilters().ToAllPredicate();
    }
}