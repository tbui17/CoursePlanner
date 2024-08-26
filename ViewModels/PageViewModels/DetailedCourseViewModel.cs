using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public partial class DetailedCourseViewModel : ObservableObject, IRefresh
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

    public ObservableCollection<Note> Notes => Course.Notes.ToObservableCollection();

    [ObservableProperty]
    private Note? _selectedNote;

    private readonly ILocalDbCtxFactory _factory;
    private readonly IAppService _appService;
    private readonly INavigationService _navService;
    private readonly ICourseService _courseService;
    private readonly ILogger<DetailedCourseViewModel> _logger;

    /// <inheritdoc/>
    public DetailedCourseViewModel(ILocalDbCtxFactory factory,
        IAppService appService,
        INavigationService navService,
        ICourseService courseService,
        ILogger<DetailedCourseViewModel> logger)
    {
        _factory = factory;
        _appService = appService;
        _navService = navService;
        _courseService = courseService;
        _logger = logger;
    }


    async partial void OnSelectedInstructorChanged(Instructor? oldValue, Instructor? newValue)
    {
        _logger.LogInformation("Selected Instructor Changed: {NewId}, {OldId}", newValue?.Id, oldValue?.Id);
        if (newValue is not { Id: > 0 })
        {
            _logger.LogInformation("Selected Instructor is null or has no id.");
            return;
        }

        if (oldValue?.Id == newValue.Id)
        {
            _logger.LogInformation("Selected Instructor is the same as the old value.");
            return;
        }

        var oldId = Course.InstructorId;
        _logger.LogInformation("Old Instructor: {InstructorId} {InstructorName}", oldId, Course.Instructor?.Name);
        var newInstructor = Instructors.First(x => x.Id == newValue.Id);
        _logger.LogInformation("New Instructor: {InstructorId} {InstructorName}", newInstructor.Id, newInstructor.Name);

        _logger.LogInformation("Course State: {CourseId} {CourseInstructorId} {CourseInstructorName}", Course.Id, Course.InstructorId, Course.Instructor?.Name);
        _logger.LogInformation("Setting Course Instructor to {InstructorId} {InstructorName}", newInstructor.Id, newInstructor.Name);
        Course.Instructor = newInstructor;
        Course.InstructorId = newValue.Id;
        _logger.LogInformation("Updated Course State: {CourseId} {CourseInstructorId} {CourseInstructorName}", Course.Id, Course.InstructorId, Course.Instructor?.Name);

        await using var db = await _factory.CreateDbContextAsync();
        var res = await db
           .Courses
           .Where(x => x.Id == Course.Id)
           .ExecuteUpdateAsync(x => x.SetProperty(p => p.InstructorId, newValue.Id));
        _logger.LogInformation("Updated Instructor in database: {UpdateCount}",res);
    }


    public async Task Init(int id)
    {
        Id = id;
        SelectedNote = null;
        await using var db = await _factory.CreateDbContextAsync();
        var course = await _courseService.GetFullCourse(id) ?? new();

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
        await _navService.GoToEditCoursePageAsync(Course.Id);
    }

    [RelayCommand]
    public async Task AddInstructorAsync()
    {
        await _navService.GoToAddInstructorPageAsync();
    }

    [RelayCommand]
    public async Task EditInstructorAsync()
    {
        if (SelectedInstructor is not { Id: var id and > 0 }) return;
        await _navService.GotoEditInstructorPageAsync(id);
    }

    [RelayCommand]
    public async Task DeleteInstructorAsync()
    {
        if (SelectedInstructor is not { Id: var id and > 0 }) return;
        await using var db = await _factory.CreateDbContextAsync();
        await db
           .Instructors
           .Where(x => x.Id == id)
           .ExecuteDeleteAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task DetailedAssessmentAsync()
    {
        await _navService.GoToAssessmentDetailsPageAsync(Id);
    }

    [RelayCommand]
    public async Task AddNoteAsync()
    {
        var name = await _appService.DisplayNamePromptAsync();

        if (name is null) return;

        await using var db = await _factory.CreateDbContextAsync();
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
        await _navService.GoToNoteDetailsPageAsync(id);
    }

    [RelayCommand]
    public async Task DeleteNoteAsync()
    {
        if (SelectedNote is not { Id: > 0 }) return;
        await using var db = await _factory.CreateDbContextAsync();
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

        await _appService.ShareAsync(request);
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await _navService.PopAsync();
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}