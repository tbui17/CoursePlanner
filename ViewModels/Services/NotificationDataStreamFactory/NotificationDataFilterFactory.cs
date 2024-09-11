using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using ViewModels.Domain;

namespace ViewModels.Services.NotificationDataStreamFactory;


public class NotificationDataFilterFactory(IFilterData data)
{


    private static Func<INotification, bool> CreateShouldNotifyFilter(
        ShouldNotifyIndex index)
    {
        return Filter;

        bool Filter(INotification ix) =>
            index switch
            {
                ShouldNotifyIndex.True => ix.ShouldNotify,
                ShouldNotifyIndex.False => !ix.ShouldNotify,
                _ => true,
            };
    }


    public IList<Func<INotification, bool>> CreateFilters()
    {
        var shouldNotifyFilter = CreateShouldNotifyFilter(data.NotificationSelectedIndex);
        return
        [
            x => x.Name.Contains(data.FilterText, StringComparison.CurrentCultureIgnoreCase),
            x =>
            {
                if (x is Assessment assessment)
                {
                    return $"{assessment.Type} Assessment".Contains(data.TypeFilter,
                        StringComparison.CurrentCultureIgnoreCase);
                }

                return x.GetType().Name.Contains(data.TypeFilter, StringComparison.CurrentCultureIgnoreCase);
            },
            shouldNotifyFilter
        ];
    }

    public Func<INotification, bool> CreateFilter()
    {
        return CreateFilters().ToAllPredicate();
    }
}