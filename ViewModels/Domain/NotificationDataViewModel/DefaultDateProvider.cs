using Lib.Interfaces;
using Lib.Models;

namespace ViewModels.Domain.NotificationDataViewModel;

public sealed class DefaultDateProvider : IDefaultDateProvider
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
}