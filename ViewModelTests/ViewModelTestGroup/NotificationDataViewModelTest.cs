using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using ViewModels.PageViewModels;
using ViewModelTests.TestSetup;

namespace ViewModelTests.ViewModelTestGroup;

public class NotificationDataViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        foreach (var set in Db.GetImplementingSets<INotification>())
        {
            await set.ExecuteUpdateAsync(x => x.SetProperty(y => y.ShouldNotify, true).SetProperty(y => y.Start, DateTime.Now));
        }
        Model = new NotificationDataViewModel(service:Resolve<NotificationService>());
    }

    private NotificationDataViewModel Model { get; set; }


    [Test]
    public async Task METHOD()
    {
        Model.NotificationItems.Should().NotBeNull();
        Model.NotificationItems.OfType<Assessment>().Should().NotBeEmpty();
        Model.MonthDate = DateTime.Now.AddMinutes(2);
        Model.FilterText = "Course";
        await Task.Delay(1000);
        Model.NotificationItems.OfType<Assessment>().Should().BeEmpty();


    }
}