using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace ViewModelTests;

public class DetailedCourseViewModelTests : BaseDbTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Db = GetDbFactory()();
        NavMock = new Mock<INavigationService>();
        AppMock = new Mock<IAppService>();
        Model = new DetailedCourseViewModel(factory: Resolve<ILocalDbCtxFactory>(), appService: AppMock.Object,
            navService: NavMock.Object, courseService: Resolve<ICourseService>()
        );
    }

    private Mock<IAppService> AppMock { get; set; }

    private Mock<INavigationService> NavMock { get; set; }

    private DetailedCourseViewModel Model { get; set; }

    private LocalDbCtx Db { get; set; }

    [TearDown]
    public async Task TearDown()
    {
        await Db.DisposeAsync();
    }


    [Test]
    public async Task Init_SetsPropertiesToDbValues()
    {
        const int id = 1;
        await Model.Init(id);
        var courseService = Resolve<ICourseService>();

        var res = await courseService.GetFullCourse(id);
        res
           .Should()
           .NotBeNull();
        var expected = res!;
        var expectedInstructors = await Db.Instructors.ToListAsync();

        expectedInstructors
           .Should()
           .NotBeEmpty();

        using var _ = new AssertionScope();


        Model
           .Instructors
           .Select(x => x.Name)
           .Should()
           .BeEquivalentTo(expectedInstructors.Select(x => x.Name));

        Model
           .Assessments
           .Select(x => x.Name)
           .Should()
           .BeEquivalentTo(expected.Assessments.Select(x => x.Name));


        Model
           .Course
           .Id
           .Should()
           .Be(expected.Id);

        Model
           .SelectedInstructor!
           .Name
           .Should()
           .Be(expected.Instructor!.Name);

        Model
           .Course
           .Name
           .Should()
           .Be(expected.Name);

        Model
           .Notes
           .Select(x => x.Value)
           .Should()
           .BeEquivalentTo(expected.Notes.Select(x => x.Value));

        Model
           .SelectedAssessment
           .Should()
           .BeNull();

        Model
           .SelectedNote
           .Should()
           .BeNull();
    }
}