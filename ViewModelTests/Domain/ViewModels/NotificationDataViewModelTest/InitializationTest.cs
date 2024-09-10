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
        await Model
            .WhenAnyValue(x => x.NotificationItems)
            .WhereNotNull()
            .FirstAsync();

        Model.NotificationItems.Should()
            .NotBeNullOrEmpty()
            .And.ContainItemsAssignableTo<INotification>();
    }


    [Test]
    public async Task Properties_UserInput_UpdateWithDbValues()
    {
        Model.Start = DateTime.Now.AddMinutes(2);
        const string filter = "Assessment";
        Model.FilterText = filter;


        await Model
            .WhenAnyValue(x => x.NotificationItems)
            .WhereNotNull()
            .FirstAsync(p => p.All(x => x.Name.Contains(filter)));

        using var scope = new AssertionScope();
        Model.NotificationItems.Should()
            .NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Name.Should().Contain(filter));
        Model.ItemCount.Should().BeGreaterThan(0);
    }
}