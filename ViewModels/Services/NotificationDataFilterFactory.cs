using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using ViewModels.Domain;

namespace ViewModels.Services;

public class NotificationDataFilterFactory
{
    public required string FilterText { get; init; }
    public required string TypeFilter { get; init; }
    public required ShouldNotifyIndex SelectedNotificationOptionIndex { get; init; }

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
        var shouldNotifyFilter = CreateShouldNotifyFilter(SelectedNotificationOptionIndex);
        return
        [
            x => x.Name.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase),
            x =>
            {
                if (x is Assessment assessment)
                {
                    return $"{assessment.Type} Assessment".Contains(TypeFilter,
                        StringComparison.CurrentCultureIgnoreCase);
                }

                return x.GetType().Name.Contains(TypeFilter, StringComparison.CurrentCultureIgnoreCase);
            },
            shouldNotifyFilter
        ];
    }
    
    public Func<INotification, bool> CreateFilter()
    {
        return CreateFilters().ToAllPredicate();
    }
}