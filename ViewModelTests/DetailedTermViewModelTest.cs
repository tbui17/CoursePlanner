using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Platform;
using Moq;
using ViewModels.PageViewModels;

namespace ViewModelTests;

public class DetailedTermViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new DetailedTermViewModel(
            factory: Resolve<ILocalDbCtxFactory>(),
            appService: AppMock.Object,
            navService: NavMock.Object
        );
    }


    private DetailedTermViewModel Model { get; set; }


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
        await Db.Courses.Where(x => x.TermId == 1).Take(1).ExecuteDeleteAsync();
        await Model.Init(1);
        Model
           .Courses
           .Should()
           .HaveCount(5);

        const string courseName = "My New Course";
        AppMock
           .Setup(x => x.DisplayNamePromptAsync())
           .ReturnsAsync(courseName);

        await Model.Init(1);


        await Model.AddCourseAsync();

        var dbTerm = await Db
           .Terms
           .Where(x => x.Id == 1)
           .Include(x => x.Courses)
           .FirstAsync();


        using var _ = new AssertionScope();

        dbTerm
           .Courses
           .Should()
           .ContainSingle(x => x.Name == courseName);


        Model
           .Courses
           .Should()
           .ContainSingle(x => x.Name == courseName);
    }


    [Test]
    public async Task AddCourse_LimitsMaxCourse()
    {
        // precondition
        await Model.Init(1);
        Model
           .Courses
           .Should()
           .HaveCount(6);


        const string courseName = "My New Course";
        AppMock
           .Setup(x => x.DisplayNamePromptAsync())
           .ReturnsAsync(courseName);


        await Model.Init(1);
        await Model.AddCourseAsync();

        var dbTerm = await Db
           .Terms
           .Where(x => x.Id == 1)
           .Include(x => x.Courses)
           .FirstAsync();

        using var scope = new AssertionScope();

        scope.FormattingOptions.MaxLines = 1;

        dbTerm
           .Courses
           .Should()
           .NotContain(x => x.Name == courseName)
           .And
           .HaveCount(6);


        Model
           .Courses
           .Should()
           .NotContain(x => x.Name == courseName)
           .And
           .HaveCount(6);


        AppMock.VerifyReceivedError();
    }
}