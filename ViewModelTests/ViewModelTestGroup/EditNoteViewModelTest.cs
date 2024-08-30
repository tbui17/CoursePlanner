using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.ViewModelTestGroup;

public class EditNoteViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new EditNoteViewModel(factory: DbFactory, navService: NavMock.Object, appService: AppMock.Object);
        await Model.Init(1);
    }

    private EditNoteViewModel Model { get; set; }

    [Test]
    public async Task Init_ShouldMapToDbValues()
    {

        var expected = await Db.Notes.FirstAsync();
        Model.Should().BeEquivalentTo(expected, x => x.ExcludingMissingMembers());
    }

    [Test]
    public async Task SaveAsync_PersistsChangesToDb()
    {
        const string name = "Test 123";
        const string text = "My Text 12345";
        Model.Name = name;
        Model.Text = text;
        await Model.SaveAsync();
        var note = await Db.Notes.Where(x => x.Name == name && x.Value == text).ToListAsync();
        note.Should().NotBeEmpty();

    }


    [TestCase("")]
    [TestCase("    ")]
    public async Task SaveAsync_InvalidInputs_RejectsChangesToDb(string name)
    {
        Model.Name = name;

        await Model.SaveAsync();

        using var _ = new AssertionScope();

        var note = await Db
           .Notes
           .Where(x => x.Name == name)
           .ToListAsync();

        note.Should().BeEmpty();
        AppMock.VerifyReceivedError();
    }
}