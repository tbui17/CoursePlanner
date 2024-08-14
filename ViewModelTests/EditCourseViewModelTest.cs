using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.PageViewModels;
using ViewModelTests.TestData;

namespace ViewModelTests;

public class EditCourseViewModelTest : BasePageViewModelTest
{

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        Model = new EditCourseViewModel(factory:DbFactory,navService:NavMock.Object,appService:AppMock.Object);
        await Model.Init(1);
    }

    private EditCourseViewModel Model { get; set; }


    [Test]
    public async Task Init_ShouldMapToDbValues()
    {
        var expected = await Db.Courses.FirstAsync(x => x.Id == 1);
        Model
           .Should()
           .BeEquivalentTo(expected, x => x.ExcludingMissingMembers());
    }

    [Test]
    public async Task SaveAsync_PersistsChangesToDb()
    {
        Model.Name = "Test 123";
        Model.Start = DateTime.Now;
        Model.End = DateTime.Now.AddDays(1);
        Model.SelectedStatus = Course.Completed;
        await Model.SaveAsync();
        var course = await Db
           .Courses
           .Where(x => x.Name == "Test 123")
           .ToListAsync();
        course
           .Should()
           .NotBeEmpty();
    }



    [TestCaseSource(typeof(TestParam), nameof(TestParam.NameAndDate))]
    public async Task SaveAsync_InvalidInputs_RejectsChangesToDb(string name, DateTime start, DateTime end)
    {
        Model.Name = name;
        Model.Start = start;
        Model.End = end;

        await Model.SaveAsync();

        using var _ = new AssertionScope();

        var course = await Db
           .Courses
           .Where(x => x.Name == name)
           .ToListAsync();
        course
           .Should()
           .BeEmpty();
        AppMock.VerifyReceivedError();
    }
}