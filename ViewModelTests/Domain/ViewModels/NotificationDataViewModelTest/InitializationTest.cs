using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(10000)]
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
                x.SetProperty(y => y.ShouldNotify, true)
                    .SetProperty(y => y.Start, DateTime.Now)
                    .SetProperty(y => y.End, DateTime.Now.AddMinutes(100)));
        }
    }

    [Test]
    public async Task Properties_Initialize_UpdateWithDbValues()
    {
        var model = Resolve<NotificationDataViewModel>();
        model.ChangeStartDate(DateTime.Now.AddMinutes(-2));
        await model.Should().EventuallyHave(x => x.PageResult.CurrentPageData.Count > 1);

        model.PageResult.CurrentPageData.Should()
            .NotBeNullOrEmpty()
            .And.ContainItemsAssignableTo<INotification>()
            .And.ContainItemsAssignableTo<Course>();
    }


    [Test]
    public async Task Properties_UserInput_UpdateWithDbValues()
    {
        var model = Resolve<NotificationDataViewModel>();
        model.ChangeStartDate(DateTime.Now.AddMinutes(-2));
        const string filter = "Assessment";
        model.FilterText = filter;

        await model.Should()
            .EventuallySatisfy(x =>
                x.PageResult.CurrentPageData.Should()
                    .NotBeNullOrEmpty()
                    .And.AllSatisfy(y => y.Name.Should().Contain(filter))
            );
    }

    [Test]
    public async Task Refresh_UpdatesWithDbValues()
    {
        var model = Resolve<NotificationDataViewModel>();
        await model.Should()
            .EventuallySatisfy(x => x.PageResult, x => x.CurrentPageData.Should().HaveCountGreaterThan(6));

        var setup = async () =>
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            foreach (var dbSet in db.GetDbSets<INotification>())
            {
                await dbSet.ExecuteDeleteAsync();
            }

            await db.Courses.ToListAsync().ContinueWith(x => x.Result.Should().BeEmpty());

            db.Add(new Course() { TermId = 1, Name = "Abc" });
            db.Add(new Course() { TermId = 1, Name = "Cde" });
            await db.SaveChangesAsync();
        };
        await setup();
        model.PageResult.CurrentPageData.Should().HaveCount(10);


        await model.RefreshAsync();

        await model.Should().EventuallySatisfy(x => x.PageResult, p =>
        {
            p.CurrentPageData.Should()
                .HaveCount(2)
                .And.Satisfy(x => x.Name == "Abc", x => x.Name == "Cde");

        });
    }

    [Test]
    public async Task Initialization_StartsAtPage1()
    {
        var model = Resolve<NotificationDataViewModel>();
        await model.RefreshAsync();

        await model.Should().EventuallySatisfy(x => x.CurrentPage.Should().Be(1));
    }

    [Test]
    public async Task Initialization_HasMoreThanOnePage()
    {
        var model = Resolve<NotificationDataViewModel>();
        await model.RefreshAsync();

        await model.Should().EventuallySatisfy(x => x.PageResult.TotalPageCount.Should().BeGreaterThan(1));
    }
}