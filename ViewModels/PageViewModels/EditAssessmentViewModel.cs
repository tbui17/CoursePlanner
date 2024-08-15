﻿using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public abstract partial class AssessmentFormViewModel(INavigationService navService)
    : ObservableObject, IAssessmentAssociatedForm
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private DateTime _start;

    [ObservableProperty]
    private DateTime _end;

    [ObservableProperty]
    private bool _shouldNotify;

    public ObservableCollection<string> AssessmentTypes { get; set; } = Assessment.Types.ToObservableCollection();

    [ObservableProperty]
    private string _selectedAssessmentType = Assessment.Types.First();


    string IAssessmentType.Type
    {
        get => SelectedAssessmentType;
        set => SelectedAssessmentType = value;
    }


    protected abstract Task SaveAsyncImpl();

    [RelayCommand]
    public async Task SaveAsync()
    {
        await SaveAsyncImpl();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }

    public abstract Task Init(int id);


    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}

public class EditAssessmentViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService) : AssessmentFormViewModel(navService)
{
    protected override async Task SaveAsyncImpl()
    {
        if (this.ValidateNameAndDates() is { } exc)
        {
            await appService.ShowErrorAsync(exc.Message);
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        var assessment = await db
           .Assessments
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        assessment.Assign(this);

        await db.SaveChangesAsync();
        await BackAsync();
    }

    public override async Task Init(int assessmentId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var assessment = await db
               .Assessments
               .FirstOrDefaultAsync(x => x.Id == assessmentId) ??
            new();

        this.Assign(assessment);
    }
}

public class AddAssessmentViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService,
    ILogger<AddAssessmentViewModel> logger) : AssessmentFormViewModel(navService)
{
    private readonly INavigationService _navService = navService;

    protected override async Task SaveAsyncImpl()
    {
        if (this.ValidateNameAndDates() is { } exc)
        {
            await appService.ShowErrorAsync(exc.Message);
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        var assessment = await db
           .Assessments
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        assessment.Assign(this);

        await db.SaveChangesAsync();
        await BackAsync();
    }

    public override async Task Init(int courseId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var course = await db
           .Courses
           .AsNoTracking()
           .Include(x => x.Assessments)
           .FirstAsync(x => x.Id == courseId);

        if (AddChangeLog.Create(course) is not { } changelog)
        {
            logger.LogInformation("Changelog null. Returning. {CourseId}", course.Id);
            await appService.ShowErrorAsync("Course already has 2 assessments. Returning to previous page.");
            await _navService.GoBackToDetailedTermPageAsync();
            return;
        }

        logger.LogInformation("Applying changelog to assessment. {Changelog}", changelog);

        this.Assign(changelog);
    }
}

public record AddChangeLog : IAssessmentAssociatedForm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool ShouldNotify { get; set; }
    public string Type { get; set; } = Assessment.DefaultType;
    public ObservableCollection<string> AssessmentTypes { get; set; } = [];

    public static IAssessmentForm? Create(Course course)
    {
        if (course.Assessments.Count >= 2)
        {
            return null;
        }

        IAssessmentForm assessment = course.CreateAssessment();

        if (course.Assessments.FirstOrDefault() is { } other)
        {
            assessment.EnsureOppositeType(other);
            var noAssessmentChangelog = new AddChangeLog();
            noAssessmentChangelog.Assign(assessment);
            noAssessmentChangelog.AssessmentTypes = [];
            return noAssessmentChangelog;
        }

        IAssessmentAssociatedForm oneAssessmentChangelog = new AddChangeLog();

        oneAssessmentChangelog.Assign(course.CreateAssessment());

        oneAssessmentChangelog.AssessmentTypes = [oneAssessmentChangelog.OppositeType];

        return oneAssessmentChangelog;
    }
}