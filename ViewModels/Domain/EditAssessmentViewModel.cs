using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
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
    IEnumerable<Assessment> GetDbModels();
    ObservableCollection<AssessmentItemViewModel> Assessments { get; set; }
    AssessmentItemViewModel? SelectedAssessment { get; set; }
    IAsyncRelayCommand BackCommand { get; }
    IRelayCommand DeleteAssessmentCommand { get; }
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand AddAssessmentCommand { get; }
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}

public partial class EditAssessmentViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService,
    ILogger<EditAssessmentViewModel> logger
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

    private HashSet<int> LocalDeleteLog { get; } = [];


    public IEnumerable<Assessment> GetDbModels() =>
        Enumerable
            .Select<AssessmentItemViewModel, Assessment>(Assessments, x => new Assessment().SetFromAssessmentForm<Assessment, AssessmentItemViewModel>(x));


    private static IEnumerable<DomainException> Validate(ICollection<Assessment> assessments)
    {
        var exceptions = new List<string>();
        foreach (var assessment in assessments)
        {
            if (assessment.ValidateNameAndDates() is { } exc)
            {
                exceptions.Add($"Type: '{assessment.Type}', Name: '{assessment.Name}', Message: {exc.Message}");
            }
        }

        if (ValidateUnique(assessments) is { } uniqueExc)
        {
            exceptions.Add(uniqueExc.Message);
        }

        return exceptions.Select(x => new DomainException(x));
    }

    private static DomainException? ValidateUnique(ICollection<Assessment> assessments)
    {
        return assessments.DistinctBy(x => x.Type).Count() != assessments.Count
            ? new DomainException("Assessment types must be unique.")
            : null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var assessmentCount = Assessments.Count;
        logger.LogInformation("Assessment count: {AssessmentCount}", assessmentCount);
        if (Assessments.Count == 0)
        {
            logger.LogInformation("No assessments to save.");
            await BackAsync();
            return;
        }

        var assessments = GetDbModels().ToImmutableList();

        logger.LogInformation("Validating assessments.");

        if (Validate(assessments).ToList() is { Count: > 0 } exceptions)
        {
            await ShowMessage();
            return;
        }

        logger.LogInformation("Assessments are valid. Saving changes.");

        await SaveChanges();
        await BackAsync();
        return;

        async Task ShowMessage()
        {
            var message = GetMessage();

            logger.LogInformation("Assessment validation failed: {Message}", message);
            var appMessage = "The assessments failed to meet the following criteria: \n" + message;

            await appService.ShowErrorAsync(appMessage);
        }

        string GetMessage()
        {
            return exceptions.Select(x => x.Message)
                .StringJoin(Environment.NewLine);
        }


        async Task SaveChanges()
        {
            await using var db = await factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var updateLog = await GetUpdateLog(assessments, db);

            var addLog = GetAddLog(assessments);

            var deleteLog = GetDeleteLog();


            var deleteQuery = db.Assessments
                .Where(x => deleteLog.Contains(x.Id));

            foreach (var x in updateLog)
            {
                x.dbModel.SetFromAssessmentForm(x.localModel);
            }

            foreach (var model in addLog)
            {
                db.Assessments.Add(model);
            }


            LogChanges(db.ChangeTracker, deleteLog);
            await deleteQuery.ExecuteDeleteAsync();
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }

    private void LogChanges(ChangeTracker changeTracker, List<int> deleteLog)
    {
        var addUpdateChanges = GetAddAndUpdateChanges(changeTracker);
        var deleteChanges = deleteLog.Select(x => new LogEntry(EntityState.Deleted, new Assessment { Id = x }));
        var changeMessage = addUpdateChanges
            .Concat(deleteChanges)
            .Select(x => x.ToString())
            .StringJoin(Environment.NewLine);

        logger.LogInformation("Changes: {Changes}", changeMessage);
    }

    private List<int> GetDeleteLog()
    {
        var deleteLog = LocalDeleteLog.Where(x => x is not 0).ToList();
        return deleteLog;
    }

    private static IEnumerable<Assessment> GetAddLog(ImmutableList<Assessment> assessments)
    {
        var addLog = assessments.Where(x => x.Id == 0);
        return addLog;
    }

    private static async Task<IEnumerable<(Assessment dbModel, Assessment localModel)>> GetUpdateLog(
        ImmutableList<Assessment> assessments, LocalDbCtx db)
    {
        var idsOfItemsToUpdate = assessments.Select(x => x.Id).ToList();

        var toUpdateData = await db.Assessments
            .AsTracking()
            .Where(x => idsOfItemsToUpdate.Contains(x.Id))
            .ToListAsync();

        var updateLog = from dbModel in toUpdateData
            join localModel in assessments on dbModel.Id equals localModel.Id
            select (dbModel, localModel);
        return updateLog;
    }

    private static IEnumerable<LogEntry> GetAddAndUpdateChanges(ChangeTracker tracker)
    {
        return tracker.Entries<Assessment>()
            .Select(x => new LogEntry(x.State, x.Entity));
    }

    private record LogEntry(EntityState State, Assessment Assessment)
    {
        public override string ToString()
        {
            var res = new
            {
                State, Assessment.Id, Assessment.Name, Assessment.Type, Assessment.Start, Assessment.End,
                Assessment.CourseId, Assessment.ShouldNotify
            };
            return res.ToString() ?? "";
        }
    };


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