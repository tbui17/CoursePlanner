using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

public interface IEditAssessmentViewModel : IRefreshId
{
    ObservableCollection<AssessmentItemViewModel> Assessments { get; set; }
    AssessmentItemViewModel? SelectedAssessment { get; set; }
    IRelayCommand DeleteAssessmentCommand { get; }
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand AddAssessmentCommand { get; }
}

[Inject(typeof(IEditAssessmentViewModel))]
public partial class EditAssessmentViewModel(
    ICourseService courseService,
    INavigationService navService,
    IAppService appService,
    ILogger<EditAssessmentViewModel> logger,
    IAssessmentService assessmentService
)
    : ObservableObject, IEditAssessmentViewModel
{
    [ObservableProperty]
    private ObservableCollection<AssessmentItemViewModel> _assessments = [];

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private AssessmentItemViewModel? _selectedAssessment;

    private Course Course { get; set; } = new();

    private DeleteLog LocalDeleteLog { get; } = new();


    public async Task Init(int courseId)
    {
        LocalDeleteLog.Clear();
        Id = courseId;

        var course = await courseService.GetCourseAndAssessments(courseId) ??
                     throw new UnreachableException("Course not found.");

        Course = course;


        Assessments = course
            .Assessments
            .Select(x => new AssessmentItemViewModel(x))
            .ToObservableCollection();
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var assessments = Assessments.Select(x => x.ToAssessment()).ToList();

        var res = await assessmentService.Merge(assessments, LocalDeleteLog);


        if (res.IsFailed)
        {
            await appService.ShowErrorAsync(res.ToErrorString());
            return;
        }

        await BackAsync();
    }


    [RelayCommand]
    private async Task BackAsync()
    {
        await navService.PopAsync();
    }


    [RelayCommand]
    private void DeleteAssessment()
    {
        if (SelectedAssessment is null) return;

        var assessment = SelectedAssessment;
        Assessments.Remove(assessment);
        SelectedAssessment = null;
        LocalDeleteLog.Add(assessment.Id);
    }

    [RelayCommand]
    private async Task AddAssessmentAsync()
    {
        var count = Assessments.Count;
        logger.LogInformation("Adding Assessment. {AssessmentCount}", count);
        if (Assessments.ValidateLength() is { } exc)
        {
            logger.LogInformation("Validation failed: {Message}", exc.Message);
            await appService.ShowErrorAsync(exc.Message);
            return;
        }

        var targetAssessment = Course.CreateAssessment();
        if (Assessments.SingleOrDefault() is { } remainingAssessment)
        {
            logger.LogInformation("Ensuring opposite type for assessment. {@TargetAssessment} {@RemainingAssessment}",
                targetAssessment,
                remainingAssessment
            );
            targetAssessment.EnsureOppositeType(remainingAssessment);
        }

        var vm = new AssessmentItemViewModel(targetAssessment);
        logger.LogInformation("Created ViewModel: {@ViewModel}", vm);
        Assessments.Add(vm);
        SelectedAssessment = vm;
    }
}