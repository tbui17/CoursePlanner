using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.Services;

namespace ViewModels.PageViewModels;


public partial class DetailedTermViewModel(IDbContextFactory<LocalDbCtx> factory, INavigationService navService, IAppService appService)
    : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [NotifyPropertyChangedFor(nameof(Courses))]
    [ObservableProperty]
    private Term _term = new();

    [ObservableProperty]
    private Course? _selectedCourse;


    public ObservableCollection<Course> Courses => Term.Courses.ToObservableCollection();


    public async Task Init(int id)
    {
        Id = id;
        await using var db = await factory.CreateDbContextAsync();
        var res = await db
               .Terms
               .Include(x => x.Courses)
               .ThenInclude(x => x.Instructor)
               .FirstOrDefaultAsync(x => x.Id == id) ??
            new();
        Term = res;
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }

    [RelayCommand]
    public async Task EditTermAsync()
    {
        await navService.GoToEditTermPageAsync(Id);
    }


    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }

    [RelayCommand]
    public async Task AddCourseAsync()
    {
        var name = await appService.DisplayNamePromptAsync();
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
           .Courses.Where(x => x.Id == SelectedCourse.Id)
           .ExecuteDeleteAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task DetailedCourseAsync()
    {
        if (SelectedCourse is null) return;
        await navService.GoToDetailedCoursesPageAsync(SelectedCourse.Id);
    }
}