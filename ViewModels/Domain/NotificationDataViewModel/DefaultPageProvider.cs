namespace ViewModels.Domain.NotificationDataViewModel;

public sealed class DefaultPageProvider : IDefaultPageProvider
{
    public int PageSize => 10;
}