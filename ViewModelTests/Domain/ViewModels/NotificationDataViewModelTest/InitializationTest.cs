using System.Reactive.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using FluentAssertions.Reactive;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ViewModels.Domain;
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
        model.Start = DateTime.Now.AddMinutes(-2);
        await model.WaitFor(x => x.PageResult?.CurrentPageData is not null);

        model.PageResult?.CurrentPageData.Should()
            .NotBeNullOrEmpty()
            .And.ContainItemsAssignableTo<INotification>()
            .And.ContainItemsAssignableTo<Course>();
    }


    [Test]
    public async Task Properties_UserInput_UpdateWithDbValues()
    {
        var model = Resolve<NotificationDataViewModel>();
        model.Start = DateTime.Now.AddMinutes(2);
        const string filter = "Assessment";
        model.FilterText = filter;

        await model.Should()
            .EventuallySatisfy(x =>
                x.PageResult?.CurrentPageData.Should()
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
        await using var db = await DbFactory.CreateDbContextAsync();
        foreach (var dbSet in db.GetDbSets<INotification>())
        {
            await dbSet.ExecuteDeleteAsync();
        }

        db.Add(new Course() { TermId = 1, Name = "Abc" });
        db.Add(new Course() { TermId = 1, Name = "Cde" });
        await db.SaveChangesAsync();

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

        await model.WhenAnyValue(x => x.PageResult)
            .WhereNotNull()
            .Where(x => x.CurrentPage is 1)
            .FirstAsync()
            .Observe()
            .Should()
            .CompleteAsync(5.Seconds());
    }

    [Test]
    public async Task Initialization_HasMoreThanOnePage()
    {
        var model = Resolve<NotificationDataViewModel>();
        await model.RefreshAsync();

        await model.WhenAnyValue(x => x.PageResult)
            .WhereNotNull()
            .Where(x => x.PageCount > 1)
            .FirstAsync()
            .Observe()
            .Should()
            .CompleteAsync(5.Seconds());
    }
}