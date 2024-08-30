using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.ViewModelTestGroup;

public class NotificationDataViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        foreach (var set in Db.GetDbSets<INotification>())
        {
            await set.ExecuteUpdateAsync(x =>
                x.SetProperty(y => y.ShouldNotify, true).SetProperty(y => y.Start, DateTime.Now));
        }

        Model = new NotificationDataViewModel(service: Resolve<NotificationService>());
        await Task.Delay(600);
    }

    private NotificationDataViewModel Model { get; set; }


    [Test]
    public async Task PropertiesTest()
    {
        Model.NotificationItems.Should().NotBeNull();
        Model.NotificationItems.OfType<Assessment>().Should().NotBeEmpty();
        Model.MonthDate = DateTime.Now.AddMinutes(2);
        Model.FilterText = "Course";
        await Task.Delay(1000);
        using var scope = new AssertionScope();
        Model.NotificationItems.Should()
            .NotContainItemsAssignableTo<Assessment>()
            .And.ContainItemsAssignableTo<Course>();
        Model.TotalItems.Should().BeGreaterThan(0);
        Model.ItemCount.Should().BeGreaterThan(0);
    }
}