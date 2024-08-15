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