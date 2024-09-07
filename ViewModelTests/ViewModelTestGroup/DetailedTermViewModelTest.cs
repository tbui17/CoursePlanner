using AutoFixture;
using BaseTestSetup.Lib;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ViewModels.Domain;
using ViewModels.Services;
using ViewModelTests.TestSetup;

namespace ViewModelTests.ViewModelTestGroup;

public class DetailedTermViewModelTest : BasePageViewModelTest
{
    private IFixture _fixture;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        _fixture = CreateFixture();
        _fixture.Register(Resolve<ILocalDbCtxFactory>);
        _fixture.Inject(AppMock.Object);
        _fixture.Register(Resolve<ILogger<DetailedTermViewModel>>);
        NavMock = _fixture.FreezeMock<INavigationService>();
        Model = _fixture.Create<DetailedTermViewModel>();
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