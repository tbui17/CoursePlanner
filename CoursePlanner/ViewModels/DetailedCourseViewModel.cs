using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoursePlanner.Services;
using Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class DetailedCourseViewModel(IDbContextFactory<LocalDbCtx> factory, AppService appShell)
    : ObservableObject
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

        Course = course;
        Instructors = instructors.ToObservableCollection();
        SelectedInstructor = Instructors.FirstOrDefault();
        SelectedNote = Notes.FirstOrDefault();
    }

    [RelayCommand]
    public async Task EditAsync()
    {
        await appShell.GoToEditCoursePageAsync(Course.Id);
    }

    [RelayCommand]
    public async Task AddInstructorAsync()
    {
        await appShell.GoToAddInstructorPageAsync();
    }

    [RelayCommand]
    public async Task AddAssessmentAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var assessment = Assessment.From(Course);
        db.Assessments.Add(assessment);
        await db.SaveChangesAsync();
    }

    [RelayCommand]
    public async Task DetailedAssessmentAsync()
    {
        if (SelectedAssessment is not { Id: var id and > 0 }) return;
        
        await appShell.GoToAssessmentDetailsPageAsync(id);
    }

    [RelayCommand]
    public async Task DeleteCourseAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        await db
           .Courses
           .Where(x => x.Id == Course.Id)
           .ExecuteDeleteAsync();
    }

    [RelayCommand]
    public async Task AddNoteAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var note = Note.From(Course);
        db.Notes.Add(note);
        await db.SaveChangesAsync();
    }

    [RelayCommand]
    public async Task DetailedNoteAsync()
    {
        if (SelectedNote is not { Id: var id and > 0 }) return;
        await appShell.GoToNoteDetailsPageAsync(id);
    }

    [RelayCommand]
    public async Task ShareAsync()
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await appShell.GoBackToDetailedTermPageAsync();
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}