using FluentAssertions;
using ViewModels.Domain.NotificationDataViewModel;

namespace ViewModelTests.Utils;

using Assertions = ReactiveObjectAssertions<NotificationDataViewModel>;

public static class NotificationDataViewModelAssertionExtensions
{
    public static async Task<AndConstraint<Assertions>> EventuallyBePopulated(
        this Assertions assertions)
    {
        return await assertions.EventuallyHave(x => x.PageResult is { CurrentPageData.Count: > 0 },5000);
    }
}