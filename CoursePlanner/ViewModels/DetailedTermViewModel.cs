using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner.ViewModels;

public partial class DetailedTermViewModel(IDbContextFactory<LocalDbCtx> factory, AppShellViewModel appShell)
    : ObservableObject, IQueryAttributable
{
    [NotifyPropertyChangedFor(nameof(Courses))]
    [ObservableProperty]
    private Term _term;

    [ObservableProperty]
    private Course? _selectedCourse;


    public ObservableCollection<Course> Courses => Term.Courses.ToObservableCollection();

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGet<int>("termId", out var id)) return;
        await Init(id);
    }

    public async Task Init(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var res = await db
           .Terms
           .Include(x => x.Courses)
           .ThenInclude(x => x.Instructor)
           .FirstAsync(x => x.Id == id);
        Term = res;
    }

    public async Task RefreshAsync()
    {
        await Init(Term.Id);
    }

    [RelayCommand]
    public async Task EditTermAsync()
    {
        var term = Term;


        await AppShell.GoToAsync<EditTermPage>(new Dictionary<string, object>
            {
                ["Id"] = term.Id,
                ["Name"] = term.Name,
                ["Start"] = term.Start,
                ["End"] = term.End
            }
        );
    }


    [RelayCommand]
    public async Task BackAsync()
    {
        await appShell.GoToMainPageAsync();
    }

    [RelayCommand]
    public async Task AddCourseAsync()
    {
        
        var name = await AppShell.DisplayNamePromptAsync();

        if (name is null)
        {
            return;
        }
        
        var course = Course.From(Term);
        course.Name = name;

        await using var db = await factory.CreateDbContextAsync();
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task DeleteCourseAsync()
    {
        if (SelectedCourse is null) return;
        await using var db = await factory.CreateDbContextAsync();
        await db
           .Courses
           .Where(x => x.Id == SelectedCourse.Id)
           .ExecuteDeleteAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task DetailedCourseAsync()
    {
        if (SelectedCourse is null) return;
        await appShell.GoToDetailedCoursesPageAsync(SelectedCourse.Id);
    }
}