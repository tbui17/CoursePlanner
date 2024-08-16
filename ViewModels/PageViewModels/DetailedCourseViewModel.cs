using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public partial class DetailedCourseViewModel(
    ILocalDbCtxFactory factory,
    IAppService appService,
    INavigationService navService,
    ICourseService courseService,
    ILogger<DetailedCourseViewModel> logger) : ObservableObject
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
        logger.LogInformation("Selected Instructor Changed: {NewId}, {OldId}", newValue?.Id, oldValue?.Id);
        if (newValue is not { Id: > 0 })
        {
            logger.LogInformation("Selected Instructor is null or has no id.");
            return;
        }

        if (oldValue?.Id == newValue.Id)
        {
            logger.LogInformation("Selected Instructor is the same as the old value.");
            return;
        }

        var oldId = Course.InstructorId;
        logger.LogInformation("Old Instructor: {InstructorId} {InstructorName}", oldId, Course.Instructor?.Name);
        var newInstructor = Instructors.First(x => x.Id == newValue.Id);
        logger.LogInformation("New Instructor: {InstructorId} {InstructorName}", newInstructor.Id, newInstructor.Name);

        logger.LogInformation("Course State: {CourseId} {CourseInstructorId} {CourseInstructorName}", Course.Id, Course.InstructorId, Course.Instructor?.Name);
        logger.LogInformation("Setting Course Instructor to {InstructorId} {InstructorName}", newInstructor.Id, newInstructor.Name);
        Course.Instructor = newInstructor;
        Course.InstructorId = newValue.Id;
        logger.LogInformation("Updated Course State: {CourseId} {CourseInstructorId} {CourseInstructorName}", Course.Id, Course.InstructorId, Course.Instructor?.Name);

        await using var db = await factory.CreateDbContextAsync();
        var res = await db
           .Courses
           .Where(x => x.Id == Course.Id)
           .ExecuteUpdateAsync(x => x.SetProperty(p => p.InstructorId, newValue.Id));
        logger.LogInformation("Updated Instructor in database: {UpdateCount}",res);
    }


    public async Task Init(int id)
    {
        Id = id;
        SelectedNote = null;
        SelectedAssessment = null;
        await using var db = await factory.CreateDbContextAsync();
        var course = await courseService.GetFullCourse(id) ?? new();

        var instructors = await db.Instructors.ToListAsync();
        SetInstructors();

        Course = course;


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
    public async Task DetailedAssessmentAsync()
    {
        if (SelectedAssessment is not { Id: var id and > 0 }) return;

        await navService.GoToAssessmentDetailsPageAsync(id);
    }

    [RelayCommand]
    public async Task AddNoteAsync()
    {
        var name = await appService.DisplayNamePromptAsync();

        if (name is null) return;

        await using var db = await factory.CreateDbContextAsync();
        var note = Course.CreateNote();
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
        await RefreshAsync();
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