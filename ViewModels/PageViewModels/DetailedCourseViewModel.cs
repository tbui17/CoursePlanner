using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public partial class DetailedCourseViewModel(ILocalDbCtxFactory factory, IAppService appService, INavigationService navService) : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [NotifyPropertyChangedFor(nameof(Assessments))]
    [NotifyPropertyChangedFor(nameof(Notes))]
    [ObservableProperty]
    private Course _course = new();

    [ObservableProperty]
    private ObservableCollection<Instructor> _instructors = [];


    [ObservableProperty]
    private Instructor? _selectedInstructor;

    public ObservableCollection<Assessment> Assessments => Course.Assessments.ToObservableCollection();


    [ObservableProperty]
    private Assessment? _selectedAssessment;

    public ObservableCollection<Note> Notes => Course.Notes.ToObservableCollection();

    [ObservableProperty]
    private Note? _selectedNote;


    async partial void OnSelectedInstructorChanged(Instructor? oldValue, Instructor? newValue)
    {
        if (newValue is not { Id: > 0 })
        {
            return;
        }

        if (oldValue?.Id == newValue.Id)
        {
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        await db
           .Courses
           .Where(x => x.Id == Course.Id)
           .ExecuteUpdateAsync(x => x.SetProperty(p => p.InstructorId, newValue.Id));
    }


    private async Task Init(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var course = await db
               .Courses
               .Include(x => x.Instructor)
               .Include(x => x.Assessments)
               .Include(x => x.Notes)
               .FirstOrDefaultAsync(x => x.Id == id) ??
            new();

        var instructors = await db.Instructors.ToListAsync();
        SetInstructors();

        Course = course;

        SelectedNote = Notes.FirstOrDefault();

        return;

        void SetInstructors()
        {
            var instructorCollection = instructors.ToObservableCollection();
            // must deselect or MAUI will throw unexpected range exception.
            SelectedInstructor = null;
            Instructors = instructorCollection;
            SelectedInstructor = instructorCollection.FirstOrDefault(x => x.Id == course.Instructor?.Id);
        }
    }

    [RelayCommand]
    public async Task EditAsync()
    {
        await navService.GoToEditCoursePageAsync(Course.Id);
    }

    [RelayCommand]
    public async Task AddInstructorAsync()
    {
        await navService.GoToAddInstructorPageAsync();
    }

    [RelayCommand]
    public async Task EditInstructorAsync()
    {
        if (SelectedInstructor is not { Id: var id and > 0 }) return;
        await navService.GotoEditInstructorPageAsync(id);
    }

    [RelayCommand]
    public async Task DeleteInstructorAsync()
    {
        if (SelectedInstructor is not { Id: var id and > 0 }) return;
        await using var db = await factory.CreateDbContextAsync();
        await db
           .Instructors
           .Where(x => x.Id == id)
           .ExecuteDeleteAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task AddAssessmentAsync()
    {
        var name = await appService.DisplayNamePromptAsync();

        if (name is null) return;

        await using var db = await factory.CreateDbContextAsync();
        var assessment = Assessment.From(Course);
        assessment.Name = name;
        db.Assessments.Add(assessment);
        await db.SaveChangesAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task DetailedAssessmentAsync()
    {
        if (SelectedAssessment is not { Id: var id and > 0 }) return;

        await navService.GoToAssessmentDetailsPageAsync(id);
    }

    [RelayCommand]
    public async Task DeleteAssessmentAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        await db
           .Assessments
           .Where(x => x.Id == Course.Id)
           .ExecuteDeleteAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task AddNoteAsync()
    {
        var name = await appService.DisplayNamePromptAsync();

        if (name is null) return;

        await using var db = await factory.CreateDbContextAsync();
        var note = Note.From(Course);
        note.Name = name;
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task DetailedNoteAsync()
    {
        if (SelectedNote is not { Id: var id and > 0 }) return;
        await navService.GoToNoteDetailsPageAsync(id);
    }

    [RelayCommand]
    public async Task DeleteNoteAsync()
    {
        if (SelectedNote is not { Id: > 0 }) return;
        await using var db = await factory.CreateDbContextAsync();
        await db
           .Notes
           .Where(x => x.Id == SelectedNote.Id)
           .ExecuteDeleteAsync();
    }

    [RelayCommand]
    public async Task ShareAsync()
    {
        if (SelectedNote is not { Id: > 0 }) return;


        var request = new ShareTextRequest
        {
            Title = "Share Note",
            Text = new ShareNote(SelectedNote).ToFriendlyText()
        };

        await appService.ShareAsync(request);
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.GoBackToDetailedTermPageAsync();
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}