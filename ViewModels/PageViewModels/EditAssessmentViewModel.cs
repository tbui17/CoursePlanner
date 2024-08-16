using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Text;
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
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public partial class EditAssessmentViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService,
    ILogger<EditAssessmentViewModel> logger
)
    : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private ObservableCollection<AssessmentItemViewModel> _assessments = [];

    [ObservableProperty]
    private AssessmentItemViewModel? _selectedAssessment;

    [ObservableProperty]
    private Course _course = new();


    public IEnumerable<Assessment> GetDbModels() =>
        Assessments
            .Select(x => new Assessment().Assign(x));


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

    public ObservableCollection<string> AssessmentTypes { get; } = Assessment.Types.ToObservableCollection();

    [RelayCommand]
    public async Task SaveAsync()
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

            var ids = assessments.Select(x => x.Id).ToList();

            logger.LogInformation("Updating assessments: {AssessmentIds}", ids);

            var toUpdate = await db.Assessments
                .AsTracking()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            var pairs = from dbCopy in toUpdate
                join local in assessments on dbCopy.Id equals local.Id
                select (dbCopy, local);

            foreach (var x in pairs)
            {
                x.dbCopy.Assign(x.local);
            }


            foreach (var model in assessments.Where(x => x.Id == 0))
            {
                db.Assessments.Add(model);
            }

            var changes = GetChanges(db.ChangeTracker);


            logger.LogInformation("Changes: {}{Changes}", Environment.NewLine, changes);

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }

    private static string GetChanges(ChangeTracker tracker) =>
        tracker
            .Entries<Assessment>()
            .GroupBy(x => x.State)
            .Select(group => group.Aggregate(
                new StringBuilder().AppendLine($"State: {group.Key}"),
                (sb, entry) => sb.Append('\t').AppendLine($"Id: {entry.Entity.Id}, Name: {entry.Entity.Name}")))
            .Select(x => x.ToString())
            .StringJoin(Environment.NewLine);

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }


    public async Task Init(int courseId)
    {
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
    private async Task DeleteAssessmentAsync()
    {
        if (SelectedAssessment is null)
        {
            return;
        }

        var assessment = SelectedAssessment;
        Assessments.Remove(assessment);
        SelectedAssessment = null;

        await using var db = await factory.CreateDbContextAsync();
        await db
            .Assessments
            .Where(x => x.Id == assessment.Id)
            .ExecuteDeleteAsync();
    }

    [RelayCommand]
    public async Task AddAssessmentAsync()
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

public partial class AssessmentItemViewModel
    : ObservableObject, IAssessmentForm
{
    public AssessmentItemViewModel()
    {
    }

    public AssessmentItemViewModel(Assessment assessment)
    {
        this.Assign(assessment);
    }

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

    [ObservableProperty]
    private string _selectedAssessmentType = Assessment.Types.First();

    public string Type
    {
        get => SelectedAssessmentType;
        set => SelectedAssessmentType = value;
    }

    public int CourseId { get; set; }
}