using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.Domain;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(10000)]
[NonParallelizable]
public class InitializationTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        await using var db = await DbFactory.CreateDbContextAsync();
        foreach (var set in db.GetDbSets<INotification>())
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
        await Model.WaitFor(x => x.PageResult?.CurrentPageData is not null);

        Model.PageResult?.CurrentPageData.Should()
            .NotBeNullOrEmpty()
            .And.ContainItemsAssignableTo<INotification>()
            .And.ContainItemsAssignableTo<Course>();
    }


    [Test]
    public async Task Properties_UserInput_UpdateWithDbValues()
    {
        Model.Start = DateTime.Now.AddMinutes(2);
        const string filter = "Assessment";
        Model.FilterText = filter;

        await Model.Should()
            .EventuallySatisfy(x =>
                x.PageResult?.CurrentPageData.Should()
                    .NotBeNullOrEmpty()
                    .And.AllSatisfy(y => y.Name.Should().Contain(filter))
            );
    }
}