using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.ViewModelTestGroup;

public class EditAssessmentViewModelTest : BasePageViewModelTest
{
    private EditAssessmentViewModel Model { get; set; }

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new EditAssessmentViewModel(factory: DbFactory, navService: NavMock.Object, appService: AppMock.Object,
            logger: Resolve<ILogger<EditAssessmentViewModel>>());
    }


    [Test]
    public async Task Init_SetsAssessmentsToDbValues()
    {
        const int courseId = 1;
        await Model.Init(courseId);

        var res = await FluentActions.Awaiting(() => Db.Courses
                .Where(x => x.Id == courseId)
                .Include(x => x.Assessments)
                .FirstAsync()
            )
            .Should()
            .NotThrowAsync();

        var dbCourse = res.Subject;

        using var _ = new AssertionScope();

        var vmAssessments = Model.GetDbModels().ToList();
        var dbAssessments = dbCourse.Assessments.ToList();

        vmAssessments
            .Should()
            .HaveCountGreaterOrEqualTo(dbAssessments.Count);

        vmAssessments
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo(dbAssessments.Select(x => x.Name));

        vmAssessments.Should().BeEquivalentTo(dbAssessments);

        Model.Assessments
            .Should()
            .HaveCount(2)
            .And.AllSatisfy(x =>
            {
                x.Type.Should()
                    .BeOneOf(Assessment.Types);

                x.AssessmentTypes
                    .ToList()
                    .Should()
                    .BeEquivalentTo(Assessment.Types.ToList());
            });
    }

    [Test]
    public async Task DeleteAsyncTest()
    {
        await Model.Init(1);

        Model.Assessments.Should().HaveCount(2);

        var first = Model.Assessments.ElementAt(0);
        var second = Model.Assessments.ElementAt(1);
        using var scope = new AssertionScope();
        Model.SelectedAssessment = first;
        Model.DeleteAssessmentCommand.Execute();

        Model.SelectedAssessment.Should().BeNull("First assessment needs to be unselected");
        Model.Assessments.Should().NotContain(first);
        Model.SelectedAssessment = second;
        Model.DeleteAssessmentCommand.Execute();
        Model.Assessments.Should().BeEmpty();
        Model.SelectedAssessment.Should().BeNull("Second assessment needs to be unselected");
    }

    [Test]
    public async Task SaveAsync_DuplicateTypes_ShouldShowError()
    {
        await Model.Init(1);

        var first = Model.Assessments.First();

        first.Type = Assessment.Performance;

        var second = Model.Assessments.ElementAt(1);
        second.Type = Assessment.Performance;
        first.Name = "";
        second.Name = "";

        await Model.SaveCommand.ExecuteAsync();
        AppMock.VerifyReceivedError();
        NavMock.Verify(x => x.PopAsync(), Times.Never);
    }

    [Test]
    public async Task SaveAsync_NoIssues_ShouldPersistChanges()
    {
        await Model.Init(1);

        const string firstName = "MY FIRST ASSESSMENT NAME";
        const string secondName = "MY SECOND ASSESSMENT NAME";

        var first = Model.Assessments.First();
        first.Type = Assessment.Performance;
        first.Name = firstName;

        var second = Model.Assessments.ElementAt(1);
        second.Type = Assessment.Objective;
        second.Name = secondName;

        await Model.SaveCommand.ExecuteAsync();


        using var _ = new AssertionScope();
        var dbAssessments = await Db.Assessments.Where(x => x.CourseId == 1).ToListAsync();

        dbAssessments.Should()
            .HaveCount(2)
            .And.ContainSingle(x => x.Name == firstName && x.Type == Assessment.Performance)
            .And.ContainSingle(x => x.Name == secondName && x.Type == Assessment.Objective)
            .And.BeEquivalentTo(Model.Assessments, o => o.ExcludingMissingMembers());

        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.PopAsync());
    }

    [Test]
    public async Task Delete_NoItems_ShouldNotThrow()
    {

        await DeleteAssessments();
        await Model.Init(1);
        Model.DeleteAssessmentCommand.Execute();
        await Model.SaveCommand.Awaiting(x => x.ExecuteAsync()).Should().NotThrowAsync();

    }

    [Test]
    public async Task SaveAsync_NoItems_ShouldNotThrow()
    {

        await DeleteAssessments();
        await Model.Init(1);
        await Model.SaveCommand.Awaiting(x => x.ExecuteAsync()).Should().NotThrowAsync();

    }

    [Test]
    public async Task AddAssessmentTest()
    {
        await DeleteAssessments();
        await Model.Init(1);

        using var _ = new AssertionScope();

        await Model.AddAssessmentCommand.ExecuteAsync();
        Model.Assessments.Should().HaveCount(1);
        Model.Assessments.First().Type.Should().Be(Assessment.Objective);

        await Model.AddAssessmentCommand.ExecuteAsync();
        Model.Assessments.Should().HaveCount(2);
        Model.Assessments.DistinctBy(x => x.Type).Should().HaveCount(2);

        await Model.AddAssessmentCommand.ExecuteAsync();
        Model.Assessments.Should().HaveCount(2);
        AppMock.VerifyReceivedError();
        NavMock.Verify(x => x.PopAsync(), Times.Never);
    }

    [Test]
    public async Task AddAndEditTest()
    {
        await DeleteAssessments();
        await Model.Init(1);
        const string name = "MY ASSESSMENT NAME";

        using var _ = new AssertionScope();

        await Model.AddAssessmentCommand.ExecuteAsync();
        var assessment = Model.Assessments.First();
        assessment.Name = name;
        await Model.SaveCommand.ExecuteAsync();
        var dbAssessment = Db.Assessments.FirstOrDefaultAsync(x => x.Name == name);

        dbAssessment.Should().NotBeNull();

        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.PopAsync(), Times.Once);
    }

    [Test]
    public async Task AddAndDeleteTest()
    {
        await DeleteAssessments();
        await Model.Init(1);
        const string name = "MY ASSESSMENT NAME";

        using var _ = new AssertionScope();

        await Model.AddAssessmentCommand.ExecuteAsync();
        await Model.AddAssessmentCommand.ExecuteAsync();
        Model.SelectedAssessment = Model.Assessments.First();
        Model.DeleteAssessmentCommand.Execute();
        Model.Assessments.Should().HaveCount(1, "First delete");
        var assessment = Model.Assessments.First();
        assessment.Name = name;
        await Model.SaveCommand.ExecuteAsync();
        var dbAssessment = Db.Assessments.FirstOrDefaultAsync(x => x.Name == name);

        dbAssessment.Should().NotBeNull();

        Model.Assessments.Should().HaveCount(1, "Second delete");

        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.PopAsync(), Times.Once);
    }


    [Test]
    public async Task AddEditAndDeleteTest()
    {
        await Model.Init(1);
        const string name = "MY ASSESSMENT NAME";
        const string name2 = "MY SECOND ASSESSMENT NAME";


        var first = Model.Assessments.ElementAt(0);
        var second = Model.Assessments.ElementAt(1);
        first.Name = name;
        Model.SelectedAssessment = second;
        Model.DeleteAssessmentCommand.Execute();
        Model.Assessments.Should().HaveCount(1);
        Model.SelectedAssessment.Should().BeNull();


        AppMock.VerifyReceivedError(0, "0");
        await Model.AddAssessmentCommand.ExecuteAsync();
        await Model.SaveCommand.ExecuteAsync();
        AppMock.VerifyReceivedError(1, "1");


        NavMock.Verify(x => x.PopAsync(), Times.Never, "2");

        var newAdd = Model.Assessments.First(x => x.Name is not name);


        newAdd.Name = name2;
        await Model.SaveCommand.ExecuteAsync();
        Model.Assessments.Count.Should().Be(2);
        AppMock.VerifyReceivedError(1, "3");

        NavMock.Verify(x => x.PopAsync(), Times.Once, "4");
    }

    private async Task DeleteAssessments()
    {
        await Db.Assessments.Where(x => x.CourseId == 1).ExecuteDeleteAsync();
    }

    [Test]
    public async Task SaveAsync_NoAssessments_ShouldNavigateBack()
    {
        await DeleteAssessments();

        await Model.Init(1);
        await Model.SaveCommand.ExecuteAsync();

        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.PopAsync());
    }
}

public static class CommandExtensions
{
    public static async Task ExecuteAsync(this IAsyncRelayCommand command)
    {
        await command.ExecuteAsync(null);
    }

    public static void Execute(this IRelayCommand command)
    {
        command.Execute(null);
    }
}