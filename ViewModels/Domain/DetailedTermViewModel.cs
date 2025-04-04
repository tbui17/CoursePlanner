﻿using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

[Inject]
public partial class DetailedTermViewModel(ILocalDbCtxFactory factory, INavigationService navService, IAppService appService, ILogger<DetailedTermViewModel> logger)
    : ObservableObject, IRefreshId
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
        logger.LogDebug("Init triggered {Id}",id);
        Id = id;
        SelectedCourse = null;
        await using var db = await factory.CreateDbContextAsync();
        var res = await db
               .Terms
               .Include(x => x.Courses)
               .ThenInclude(x => x.Instructor)
               .AsNoTracking()
               .AsSplitQuery()
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

        await using var db = await factory.CreateDbContextAsync();

        using var _ = logger.BeginScope("{Method}", nameof(AddCourseAsync));

        var courseCount = await db.Courses.CountAsync(x => x.TermId == Term.Id);
        logger.LogDebug("Course count {CourseCount}",courseCount);
        if (courseCount >= 6)
        {
            await appService.ShowErrorAsync("You can only have up to 6 courses per term");
            await RefreshAsync();
            return;
        }

        var name = await appService.DisplayNamePromptAsync();
        logger.LogDebug("{Name}",name);
        if (name is null)
        {
            return;
        }

        var course = Term.CreateCourse();
        course.Name = name;
        logger.LogDebug("{Course}",course);

        db.Courses.Add(course);
        await db.SaveChangesAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task DeleteCourseAsync()
    {
        logger.LogDebug("Delete triggered {SelectedCourse}",SelectedCourse);
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
        logger.LogDebug("Detailed course triggered {SelectedCourse}",SelectedCourse);
        if (SelectedCourse is null) return;
        await navService.GoToDetailedCoursesPageAsync(SelectedCourse.Id);
    }
}