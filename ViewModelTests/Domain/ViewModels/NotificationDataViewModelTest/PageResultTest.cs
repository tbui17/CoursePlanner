using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class PageResultTest : BaseTest
{
    private static NotificationsInputData CreateReturnedData()
    {
        var data = CreateNotificationData();
        // should cause no filters to be applied
        var input = new NotificationsInputData
        {
            Notifications = data,
            InputData = new PartialInputData
            {
                FilterText = "",
                TypeFilter = "",
                NotificationSelectedIndex = ShouldNotifyIndex.None,
                CurrentPage = 1,
                PageSize = 10,
            }
        };

        return input;
    }

    private static IList<INotification> CreateNotificationData()
    {
        var dateRange = new DateTimeRange
        {
            Start = new DateTime(2000, 1, 1),
            End = new DateTime(2000, 2, 1)
        };

        var data = Enumerable.Range(1, 50)
            .Select(x => new NotificationData
                {
                    Id = x,
                    Name = $"Notification {x}",
                    Start = dateRange.Start,
                    End = dateRange.End,
                    ShouldNotify = true
                }
            )
            .Cast<INotification>()
            .ToList()
            .As<IList<INotification>>();
        return data;
    }

    [Test]
    public void AssertTestDataValid()
    {
        var data = CreateReturnedData();
        data.ValidateFull();
    }

    [Test]
    public void PageCount_NoFilters50Items10PageSize_HasExpectedProperties()
    {
        var data = CreateReturnedData();
        IPageResult x = Resolve<PageResultFactory>()
            .Create(data);

        using var _ = new AssertionScope();

        x.PageCount.Should().Be(5);
        x.ItemCount.Should().Be(10);
        x.CurrentPage.Should().Be(1);
        x.CurrentPageData.Should().HaveCount(10);
        x.CurrentPageData.Should().BeSubsetOf(data.Notifications);
        x.HasPrevious.Should().BeFalse();
        x.HasNext.Should().BeTrue();
    }
}

file static class ValidationExtensions
{
    public static void Validate(this INotification data)
    {
        using var _ = new AssertionScope();
        data.Start.Should().BeBefore(data.End);
        data.Name.Should().NotBeNullOrEmpty();
        data.Id.Should().BePositive().And.NotBe(0);
    }

    public static void Validate(this NotificationsInputData rdata)
    {
        var data = rdata.InputData;
        using var _ = new AssertionScope();
        data.CurrentPage.Should().BePositive().And.NotBe(0);
        data.PageSize.Should().BePositive().And.NotBe(0);
        data.FilterText.Should().NotBeNull();
        data.TypeFilter.Should().NotBeNull();
        data.NotificationSelectedIndex.Should().Be(ShouldNotifyIndex.None);
    }

    public static void ValidateFull(this NotificationsInputData data)
    {
        using var _ = new AssertionScope();
        data.Validate();
        data.Notifications
            .Should()
            .NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Validate())
            .And.HaveCount(50);
    }
}