using System.Reactive.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(5000)]
public class InitializationTest : BasePageViewModelTest
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
    }

    private NotificationDataViewModel Model { get; set; }

    [Test]
    public async Task Properties_Initialize_UpdateWithDbValues()
    {
        var res = await Model
            .WhenAnyValue(x => x.NotificationItems)
            .WhereNotNull()
            .Take(1);

        res.Should()
            .NotBeNullOrEmpty()
            .And.ContainItemsAssignableTo<Assessment>();
    }


    [Test]
    public async Task Properties_UserInput_UpdateWithDbValues()
    {
        Model.Start = DateTime.Now.AddMinutes(2);
        Model.FilterText = "Course";


        var items = await Model
            .WhenAnyValue(x => x.NotificationItems)
            .WhereNotNull()
            .TakeUntil(x => x.OfType<Course>().Any());

        using var scope = new AssertionScope();
        items.Should()
            .NotContainItemsAssignableTo<Assessment>()
            .And.ContainItemsAssignableTo<Course>();
        Model.ItemCount.Should().BeGreaterThan(0);
    }
}