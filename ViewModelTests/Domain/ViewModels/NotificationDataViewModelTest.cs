using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels;

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

        Model = Resolve<NotificationDataViewModel>();
        await Task.Delay(600);
    }

    private NotificationDataViewModel Model { get; set; }

    [Test]
    public async Task Properties_Initialize_UpdateWithDbValues()
    {
        Model.NotificationItems.Should()
            .NotBeNull()
            .And.ContainItemsAssignableTo<Assessment>();
        await Task.CompletedTask;
    }


    [Test]
    public async Task Properties_UserInput_UpdateWithDbValues()
    {

        await Model.RefreshAsync();
        Model.Start = DateTime.Now.AddMinutes(2);
        Model.FilterText = "Course";
        await Task.Delay(1000);
        using var scope = new AssertionScope();
        Model.NotificationItems.Should()
            .NotContainItemsAssignableTo<Assessment>()
            .And.ContainItemsAssignableTo<Course>();
        Model.ItemCount.Should().BeGreaterThan(0);
    }
}