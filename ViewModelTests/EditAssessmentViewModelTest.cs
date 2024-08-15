using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using ViewModels.PageViewModels;
using ViewModelTests.TestData;

namespace ViewModelTests;

public class EditAssessmentViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new EditAssessmentViewModel(factory: DbFactory, navService: NavMock.Object, appService: AppMock.Object);
        await Model.Init(1);
    }

    private EditAssessmentViewModel Model { get; set; }

    [Test]
    public async Task Init_ShouldMapToDbValues()
    {

        var expected = await Db.Assessments.FirstAsync();
        Model.Should().BeEquivalentTo(expected, x => x.ExcludingMissingMembers());
    }

    [Test]
    public async Task SaveAsync_PersistsChangesToDb()
    {
        Model.Name = "Test 123";
        Model.Start = DateTime.Now;
        Model.End = DateTime.Now.AddDays(1);
        await Model.SaveAsync();
        var assessment = await Db.Assessments.Where(x => x.Name == "Test 123").ToListAsync();
        assessment.Should().NotBeEmpty();

    }


    [TestCaseSource(typeof(TestParam), nameof(TestParam.NameAndDate))]
    public async Task SaveAsync_InvalidInputsIDateTimeEntity_RejectsChangesToDb(string name, DateTime start, DateTime end)
    {
        Model.Name = name;
        Model.Start = start;
        Model.End = end;

        await Model.SaveAsync();



        var assessment = await Db
           .Assessments
           .Where(x => x.Name == name)
           .ToListAsync();

        using var _ = new AssertionScope();

        assessment.Should().BeEmpty();
        AppMock.VerifyReceivedError();
    }

    [Test]
    public async Task SaveAsync_InvalidInputsObjectiveAssessment_RejectsChangesToDb()
    {
        Model.Name = "Test 123";
        Model.Start = DateTime.Now;
        Model.End = Model.Start.AddDays(1);

        await Model.SaveAsync();



        var assessment = await Db
           .Assessments
           .Where(x => x.Name == Model.Name)
           .ToListAsync();

        using var _ = new AssertionScope();

        assessment.Should().BeEmpty();
        AppMock.VerifyReceivedError();
    }

    [Test]
    public async Task SaveAsync_InvalidInputsPerformanceAssessment_RejectsChangesToDb()
    {
        Model.Name = "Test 123";
        Model.Start = DateTime.Now;
        Model.End = Model.Start.AddDays(1);

        await Model.SaveAsync();



        var assessment = await Db
           .Assessments
           .Where(x => x.Name == Model.Name)
           .ToListAsync();

        using var _ = new AssertionScope();

        assessment.Should().BeEmpty();
        AppMock.VerifyReceivedError();
    }
}

//
// [Description("run both tests at once and both must pass")]
// public class AssessmentFormTest : BasePageViewModelTest
// {
//     private Assessment ObjectiveAssessment { get; set; }
//     private Assessment PerformanceAssessment { get; set; }
//     private EditAssessmentViewModel Model { get; set; }
//
//     [SetUp]
//     public override async Task Setup()
//     {
//         await base.Setup();
//         Model = new EditAssessmentViewModel(
//             factory: DbFactory,
//             appService: AppMock.Object,
//             navService: NavMock.Object
//         );
//
//         // remove assessments and assign their references
//         // should have 1 objective and 1 performance
//
//         await using var db = await DbFactory.CreateDbContextAsync();
//
//         var assessments = await db
//            .Assessments
//            .AsNoTracking()
//            .Where(x => x.CourseId == 1)
//            .ToListAsync();
//
//         const int assessmentCount = 2;
//
//         assessments
//            .Should()
//            .HaveCount(assessmentCount)
//            .And
//            .Subject
//            .Where(x => x.Type is Assessment.Objective or Assessment.Performance)
//            .Should()
//            .HaveCount(assessmentCount);
//
//         var ids = assessments
//            .Select(x => x.Id)
//            .ToList();
//
//         var res = await db
//            .Assessments
//            .Where(x => ids.Contains(x.Id))
//            .ExecuteDeleteAsync();
//         res
//            .Should()
//            .Be(assessmentCount);
//
//         ObjectiveAssessment = assessments.First(x => x.Type == Assessment.Objective);
//         PerformanceAssessment = assessments.First(x => x.Type == Assessment.Performance);
//     }
//
//     private async Task AddAssessmentTestImpl(Assessment assessment)
//     {
//         Db.Assessments.Add(assessment);
//         await Db.SaveChangesAsync();
//
//         const int assessmentId = 1;
//         const string newAssessmentName = "My New Assessment12345";
//         // AppMock
//         //    .Setup(x => x.DisplayNamePromptAsync())
//         //    .ReturnsAsync(newAssessmentName);
//
//         await Model.Init(assessmentId);
//         await Model.SaveAsync();
//
//         using var scope = new AssertionScope();
//         scope.FormattingOptions.MaxLines = 1;
//
//         Model
//            .Assessments
//            .Where(x => x.Type is Assessment.Objective or Assessment.Performance)
//            .Should()
//            .HaveCount(2)
//            .And
//            .ContainSingle(x => x.Name == newAssessmentName)
//            .And
//            .ContainSingle(x => x.Type == assessment.Type);
//     }
//
//
//     [Test]
//     public async Task AddAssessmentAsync_Performance_DefaultsWithUniqueType()
//     {
//         await AddAssessmentTestImpl(PerformanceAssessment);
//     }
//
//     [Test]
//     public async Task AddAssessmentAsync_Objective_DefaultsWithUniqueType()
//     {
//         await AddAssessmentTestImpl(ObjectiveAssessment);
//     }
// }