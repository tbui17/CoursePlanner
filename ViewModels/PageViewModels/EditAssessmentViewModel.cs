using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public abstract partial class AssessmentFormViewModel(INavigationService navService)
    : ObservableObject, IAssessmentForm
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

    public ObservableCollection<string> AssessmentTypes { get; } = Assessment.Types.ToObservableCollection();

    [ObservableProperty]
    private string _selectedAssessmentType = Assessment.Types.First();

    string IAssessmentForm.Type
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

    public override async Task Init(int courseId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var course = await db
           .Courses
           .AsNoTracking()
           .Include(x => x.Assessments)
           .FirstAsync(x => x.Id == courseId);

        this.Assign(course);
    }
}