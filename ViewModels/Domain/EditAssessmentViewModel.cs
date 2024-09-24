using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using Lib.Traits;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;
using ViewModels.Services;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace ViewModels.Domain;

public interface IEditAssessmentViewModel : IRefreshId
{
    int Id { get; set; }
    IEnumerable<Assessment> CreateDbModels();
    ObservableCollection<AssessmentItemViewModel> Assessments { get; set; }
    AssessmentItemViewModel? SelectedAssessment { get; set; }
    IAsyncRelayCommand BackCommand { get; }
    IRelayCommand DeleteAssessmentCommand { get; }
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand AddAssessmentCommand { get; }
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}

[Inject(typeof(IEditAssessmentViewModel))]
public partial class EditAssessmentViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService,
    ILogger<EditAssessmentViewModel> logger,
    IAssessmentService assessmentService
)
    : ObservableObject, IEditAssessmentViewModel
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private ObservableCollection<AssessmentItemViewModel> _assessments = [];

    [ObservableProperty]
    private AssessmentItemViewModel? _selectedAssessment;

    private Course Course { get; set; } = new();

    private DeleteLogCollection LocalDeleteLog { get; } = new();


    public IEnumerable<Assessment> CreateDbModels() => Assessments.Select(x => x.ToAssessment());

    private bool HasNoChanges => Assessments.Count == 0 && LocalDeleteLog.IsEmpty;

    [RelayCommand]
    private async Task SaveAsync()
    {
        logger.LogInformation("Assessment count: {AssessmentCount}", Assessments.Count);
        if (HasNoChanges)
        {
            logger.LogInformation("No assessments to save.");
            await BackAsync();
            return;
        }

        var assessments = CreateDbModels().ToImmutableList();

        logger.LogInformation("Validating assessments.");

        if (assessments.GetUniqueValidationException() is {} exc)
        {
            logger.LogInformation("Assessment validation failed: {Message}", exc.Message);
            await appService.ShowErrorAsync(exc.Message);
            return;
        }

        logger.LogInformation("Assessments are valid. Saving changes.");

        await assessmentService.SaveChanges(assessments,LocalDeleteLog);
        await BackAsync();
        return;


    }





    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }


    public async Task Init(int courseId)
    {
        LocalDeleteLog.Clear();
        Id = courseId;

        await using var db = await factory.CreateDbContextAsync();

        var course = await db
            .Courses
            .Where(x => x.Id == courseId)
            .Include(x => x.Assessments)
            .FirstAsync();

        Course = course;

        var assessments = course.Assessments;

        Assessments = assessments
            .Select(x => new AssessmentItemViewModel(x))
            .ToObservableCollection();
    }


    [RelayCommand]
    private void DeleteAssessment()
    {
        if (SelectedAssessment is null)
        {
            return;
        }

        var assessment = SelectedAssessment;
        Assessments.Remove(assessment);
        SelectedAssessment = null;
        LocalDeleteLog.Add(assessment.Id);
    }

    [RelayCommand]
    private async Task AddAssessmentAsync()
    {
        logger.LogInformation("Adding Assessment. {AssessmentCount}", Assessments.Count);
        if (Assessments.Count >= 2)
        {
            logger.LogInformation("{AssessmentCount} assessments already exist.", Assessments.Count);
            await appService.ShowErrorAsync("Only 2 assessments allowed per course.");
            return;
        }

        var assessment = CreateAssessment();
        var vm = new AssessmentItemViewModel(assessment);
        Assessments.Add(vm);
        SelectedAssessment = vm;
        return;

        Assessment CreateAssessment()
        {
            var course = Course;
            var newAssessment = course.CreateAssessment();

            switch (Assessments.Count)
            {
                case >= 2:
                    throw new UnreachableException("There should not be more than 2 assessments.");
                case 0:
                    return newAssessment;
                default:
                    var remainingAssessment = Assessments[0];

                    newAssessment.EnsureOppositeType(remainingAssessment);

                    return newAssessment;
            }
        }
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}