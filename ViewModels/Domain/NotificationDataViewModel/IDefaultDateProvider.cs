using Lib.Interfaces;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface IDefaultDateProvider
{
    IDateTimeRange DateRange { get; }
}