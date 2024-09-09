using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using ViewModels.Domain;
using ViewModelTests.TestData;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain;

public class EditTermViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new EditTermViewModel(factory: DbFactory, navService: NavMock.Object, appService: AppMock.Object);
        await Model.Init(1);
    }

    private EditTermViewModel Model { get; set; }

    [Test]
    public async Task Init_ShouldMapToDbValues()
    {

        var expected = await Db.Terms.FirstAsync();
        Model.Should().BeEquivalentTo(expected, x => x.ExcludingMissingMembers());
    }

    [Test]
    public async Task SaveAsync_PersistsChangesToDb()
    {
        Model.Name = "Test 123";
        Model.Start = DateTime.Now;
        Model.End = DateTime.Now.AddDays(1);
        await Model.SaveAsync();
        var term = await Db.Terms.Where(x => x.Name == "Test 123").ToListAsync();
        term.Should().NotBeEmpty();

    }


    [TestCaseSource(typeof(TestParam), nameof(TestParam.NameAndDate))]
    public async Task SaveAsync_InvalidInputs_RejectsChangesToDb(string name, DateTime start, DateTime end)
    {
        Model.Name = name;
        Model.Start = start;
        Model.End = end;

        await Model.SaveAsync();

        using var _ = new AssertionScope();

        var term = await Db
           .Terms
           .Where(x => x.Name == name)
           .ToListAsync();

        term.Should().BeEmpty();
        AppMock.VerifyReceivedError();
    }
}