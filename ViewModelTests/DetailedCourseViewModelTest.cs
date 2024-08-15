using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace ViewModelTests;

public class DetailedCourseViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new DetailedCourseViewModel(factory: DbFactory, appService: AppMock.Object,
            navService: NavMock.Object, courseService: Resolve<ICourseService>()
        );
    }

    private DetailedCourseViewModel Model { get; set; }


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

    [Test]
    public async Task ShareAsync_SelectedNote_SendsShareTextRequestWithNonEmptyStringToApp()
    {
        var args = new List<ShareTextRequest>();
        AppMock.Setup(x => x.ShareAsync(Capture.In(args)));
        await Model.Init(1);

        var note = Model.Notes.First();
        Model.SelectedNote = note;

        await Model.ShareAsync();

        using var _ = new AssertionScope();

        var request = args
           .Should()
           .ContainSingle()
           .Subject;

        request
           .Text
           .Should()
           .NotBeNullOrEmpty();

        request
           .Title
           .Should()
           .NotBeNullOrEmpty();
    }

    [Test]
    public async Task ShareAsync_SelectedNote_ShareRequestContainsNoteContents()
    {
        var args = new List<ShareTextRequest>();
        AppMock.Setup(x => x.ShareAsync(Capture.In(args)));

        await Model.Init(1);
        var note = Model.Notes.First();
        Model.SelectedNote = note;

        await Model.ShareAsync();

        using var _ = new AssertionScope();


        var textRequest = args
           .Should()
           .ContainSingle()
           .Subject;

        textRequest
           .Text
           .Should()
           .Contain(note.Value);
    }


    [Test]
    public async Task DeleteNoteAsync_DbAndModelStateUpdated()
    {
        var initialDbNotes = await Db
           .Notes
           .AsNoTracking()
           .ToListAsync();
        await Model.Init(1);
        var note = Model.Notes.First();
        var noteId = note.Id;
        var noteCount = Model.Notes.Count;

        var expectedNoteCount = noteCount - 1;
        var expectedDbNoteCount = initialDbNotes.Count - 1;


        Model.SelectedNote = note;
        await Model.DeleteNoteAsync();

        var dbNotes = await Db
           .Notes
           .AsNoTracking()
           .ToListAsync();

        using var scope = new AssertionScope();
        scope.FormattingOptions.MaxLines = 0;

        dbNotes
           .Should()
           .NotContain(x => x.Id == noteId)
           .And
           .HaveCount(expectedDbNoteCount);

        Model
           .Notes
           .Should()
           .NotContain(x => x.Id == noteId)
           .And
           .HaveCount(expectedNoteCount);

        Model
           .SelectedNote
           .Should()
           .BeNull();
    }


    [Test]
    public async Task AddNoteAsync_UpdatesDbAndModelState()
    {
        const string newNoteName = "My New Note ABC";
        AppMock
           .Setup(x => x.DisplayNamePromptAsync())
           .ReturnsAsync(newNoteName);

        await Model.Init(1);
        await Model.AddNoteAsync();


        var dbNotes = await Db
           .Courses
           .Where(x => x.Id == 1)
           .Include(x => x.Notes)
           .Select(x => x.Notes)
           .FirstAsync();

        using var _ = new AssertionScope();

        dbNotes
           .Should()
           .ContainSingle(x => x.Name == newNoteName);

        Model
           .Notes
           .Should()
           .ContainSingle(x => x.Name == newNoteName);
    }


    private class InstructorSelectionTests : BaseDbTest
    {
        private const int InstructorId = 1;
        private const int NewInstructorId = 2;
        private const int CourseId = 1;

        [SetUp]
        public override async Task Setup()
        {
            await base.Setup();
            Db = GetDb();
            NavMock = new Mock<INavigationService>();
            AppMock = new Mock<IAppService>();
            Model = new DetailedCourseViewModel(factory: Resolve<ILocalDbCtxFactory>(), appService: AppMock.Object,
                navService: NavMock.Object, courseService: Resolve<ICourseService>()
            );
            await SetInitialDbAndModelStates();
        }

        private async Task SetInitialDbAndModelStates()
        {
            await Db.Courses.ExecuteUpdateAsync(x => x.SetProperty(p => p.InstructorId, InstructorId));
            await Model.Init(CourseId);
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
        public void FixtureInitializedWithCorrectValues()
        {
            Model
               .Course
               .Id
               .Should()
               .Be(CourseId);
            Model
               .Id
               .Should()
               .Be(CourseId);
            Model
               .Course
               .InstructorId
               .Should()
               .Be(InstructorId);

            Model
               .SelectedInstructor
              ?.Id
               .Should()
               .Be(InstructorId);
        }


        [Test]
        public async Task OnSelectedInstructorChanged_NewId_ModelHasUpdatedValues()
        {
            Model.SelectedInstructor = Model.Instructors.First(x => x.Id == NewInstructorId);

            var updatedCourse = await Db
               .Courses
               .AsNoTracking()
               .FirstAsync(x => x.Id == CourseId);

            updatedCourse
               .InstructorId
               .Should()
               .Be(NewInstructorId);

            Model
               .SelectedInstructor
              ?.Id
               .Should()
               .Be(NewInstructorId);

            Model
               .Course
               .InstructorId
               .Should()
               .Be(NewInstructorId);
        }

        [Test]
        public async Task DeleteInstructorAsync_ModelHasNullInstructor()
        {
            await Model.DeleteInstructorAsync();
            using var _ = new AssertionScope();
            Model
               .SelectedInstructor
               .Should()
               .BeNull();
            Model
               .Course
               .Instructor
               .Should()
               .BeNull();
            Model
               .Course
               .InstructorId
               .Should()
               .Be(null);
        }
    }
}

public class DetailedCourseViewModelAssessmentTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new DetailedCourseViewModel(factory: DbFactory, appService: AppMock.Object,
            navService: NavMock.Object, courseService: Resolve<ICourseService>()
        );

        var assessments = await Db
           .Assessments
           .AsNoTracking()
           .Where(x => x.CourseId == 1)
           .ToListAsync();

        const int assessmentCount = 2;

        assessments
           .Should()
           .HaveCount(assessmentCount)
           .And
           .Subject
           .Where(x => x.Type is Assessment.Objective or Assessment.Performance)
           .Should()
           .HaveCount(assessmentCount);

        var ids = assessments
           .Select(x => x.Id)
           .ToList();

        var res = await Db
           .Assessments
           .Where(x => ids.Contains(x.Id))
           .ExecuteDeleteAsync();
        res
           .Should()
           .Be(assessmentCount);
    }

    public DetailedCourseViewModel Model { get; set; }


    [Test]
    public async Task AddAssessmentAsync_DefaultsWithUniqueType()
    {
        await Model.Init(1);
    }
}