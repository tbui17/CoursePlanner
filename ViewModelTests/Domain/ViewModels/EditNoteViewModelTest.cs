using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels;

public class EditNoteViewModelTest : BasePageViewModelTest
{
    private const int NoteId = 1;
    private IEditNoteViewModel Model { get; set; }

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = new EditNoteViewModel(factory: DbFactory, navService: NavMock.Object, appService: AppMock.Object);
        await Model.Init(NoteId);
    }

    [Test]
    public async Task SaveAsync_Valid_AcceptsUpdate()
    {
        // Given a valid note
        const string name = "Test 123";
        const string text = "My Text 12345";
        Model.Name = name;
        Model.Text = text;

        // When the user saves it
        await Model.SaveCommand.ExecuteAsync();

        // Then the updates are accepted
        var notes = await Db.Notes.Where(x => x.Name == name && x.Value == text && x.Id == NoteId).ToListAsync();

        notes.Should().ContainSingle();
        AppMock.VerifyReceivedNoError();
        NavMock.Verify(x => x.PopAsync());
    }

    [Test]
    public async Task SaveAsync_Valid_SavesChangesToDatabase()
    {
        // Given a valid note
        const string name = "Test 123";
        const string text = "My Text 12345";
        Model.Name = name;
        Model.Text = text;

        // When the user saves it
        await Model.SaveCommand.ExecuteAsync();

        // Then the updates are accepted
        var notes = await Db.Notes.Where(x => x.Name == name && x.Value == text && x.Id == NoteId).ToListAsync();

        notes.Should().ContainSingle();
    }

    [TestCase("")]
    [TestCase("    ")]
    public async Task SaveAsync_InvalidName_RejectsUpdate(string name)
    {
        // Given a note with an invalid name of blank or whitespaces
        Model.Name = name;

        // When the user attempts to save it
        await Model.SaveCommand.ExecuteAsync();

        using var _ = new AssertionScope();

        // Then the update is rejected
        var hasNoteWithName = await Db.Notes.AnyAsync(x => x.Name == name && x.Id == NoteId);

        hasNoteWithName.Should().BeFalse();
        AppMock.VerifyReceivedError();
        NavMock.Verify(x => x.PopAsync(), Times.Never);
    }
}