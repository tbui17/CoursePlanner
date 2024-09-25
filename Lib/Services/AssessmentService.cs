using System.Data;
using Lib.Attributes;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Lib.Services;

public interface IAssessmentService
{
    Task<DomainException?> Merge(IReadOnlyCollection<Assessment> assessments, DeleteLog deleteLog);
}

[Inject(typeof(IAssessmentService))]
public class AssessmentService(ILocalDbCtxFactory factory, ILogger<AssessmentService> logger) : IAssessmentService
{
    public async Task<DomainException?> Merge(IReadOnlyCollection<Assessment> assessments, DeleteLog deleteLog)
    {
        using var _ = logger.MethodScope();
        logger.LogInformation("Received Assessment count: {AssessmentCount}", assessments.Count);
        logger.LogInformation("Assessments: {@Assessments}", assessments);
        if (HasNoChanges(assessments, deleteLog))
        {
            logger.LogInformation("No changes detected.");
            return null;
        }

        logger.LogInformation("Validating assessments.");
        if (assessments.GetUniqueValidationException() is { } exc)
        {
            logger.LogInformation("Validation failed: {Message}", exc.Message);
            return exc;
        }

        logger.LogInformation("Assessments are valid. Saving changes.");

        var count = await MergeImpl(assessments, deleteLog);

        logger.LogInformation("Changes saved. Count: {Count}", count);

        return null;
    }

    private async Task<int> MergeImpl(IReadOnlyCollection<Assessment> assessments, DeleteLog deleteLog)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);


        var updateLog = await db.Assessments.JoinAsync(assessments);

        var addLog = assessments.Where(x => x.IsNew());

        var deleteData = deleteLog.Value().ToList();

        var deleteQuery = db.Assessments
            .Where(x => deleteData.Contains(x.Id));

        db.Assessments.AddRange(addLog);

        foreach (var (localModel, dbModel) in updateLog)
        {
            dbModel.SetFromAssessmentForm(localModel);
        }


        LogChanges(db.ChangeTracker, deleteData);
        var count1 = await deleteQuery.ExecuteDeleteAsync();
        var count2 = await db.SaveChangesAsync();
        await tx.CommitAsync();

        return count1 + count2;
    }

    private static bool HasNoChanges(IReadOnlyCollection<Assessment> assessments, DeleteLog localDeleteLog)
    {
        return assessments.Count == 0 && localDeleteLog.IsEmpty;
    }

    private static IEnumerable<LogEntry> GetAddAndUpdateChanges(ChangeTracker tracker)
    {
        return tracker
            .Entries()
            .Select(x => new LogEntry(x.State, x.Entity));
    }

    private void LogChanges(ChangeTracker changeTracker, List<int> deleteLog)
    {
        var addUpdateChanges = GetAddAndUpdateChanges(changeTracker);
        var deleteChanges = deleteLog.Select(x => new LogEntry(EntityState.Deleted, new Assessment { Id = x }));

        logger.LogInformation("Changes: {@Changes}", addUpdateChanges.Concat(deleteChanges));
    }

    private record LogEntry(EntityState State, object Entity);
}