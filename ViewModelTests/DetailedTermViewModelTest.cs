using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace ViewModelTests;

public class DetailedTermViewModelTest : BaseDbTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Db = GetDb();
        NavMock = new Mock<INavigationService>();
        AppMock = new Mock<IAppService>();
        Model = new DetailedTermViewModel(factory: Resolve<ILocalDbCtxFactory>(), appService: AppMock.Object, navService: NavMock.Object);
    }

    private Mock<IAppService> AppMock { get; set; }

    private Mock<INavigationService> NavMock { get; set; }

    private DetailedTermViewModel Model { get; set; }

    private LocalDbCtx Db { get; set; }


    [Test]
    public async Task Init_AssignsDbValues()
    {
        await Model.Init(1);
        var dbTerm = await Db
           .Terms
           .AsNoTracking()
           .Include(x => x.Courses)
           .ThenInclude(x => x.Instructor)
           .FirstAsync(x => x.Id == 1);

        using var _ = new AssertionScope();

        Model
           .Term
           .Should()
           .BeEquivalentTo(dbTerm, o => o.Excluding(x => x.Courses));

        Model
           .Term
           .Courses
           .Select(x => x.Name)
           .Should()
           .BeEquivalentTo(dbTerm.Courses.Select(x => x.Name));
    }

    [Test]
    public async Task AddCourse_UpdatesModelAndDbState()
    {

        const string courseName = "My New Course";
        AppMock.Setup(x => x.DisplayNamePromptAsync()).ReturnsAsync(courseName);

        await Model.Init(1);


        await Model.AddCourseAsync();

        var dbTerm = await Db
           .Terms
           .Where(x => x. Id == 1)
           .Include(x => x.Courses)
           .FirstAsync();



        using var _ = new AssertionScope();

        dbTerm
           .Courses
           .Should()
           .ContainSingle(x => x.Name == courseName);


        Model.Courses
           .Should()
           .ContainSingle(x => x.Name == courseName);

    }
}