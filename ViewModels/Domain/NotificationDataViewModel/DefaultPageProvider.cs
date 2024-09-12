using Lib.Attributes;

namespace ViewModels.Domain.NotificationDataViewModel;

[Inject(typeof(IDefaultPageProvider))]
public sealed class DefaultPageProvider : IDefaultPageProvider
{
    public int PageSize => 10;
}