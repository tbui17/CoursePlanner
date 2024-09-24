using System.Data;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Lib.Services;

public static class DbSetExtensions
{
    public static async Task<IEnumerable<(T Local, T Database)>> JoinAsync<T>(
        this DbSet<T> dbSet,
        IReadOnlyCollection<T> localModels
    ) where T : class, IDatabaseEntry
    {
        var query =
            from item in dbSet
            join id in localModels.Select(x => x.Id) on item.Id equals id
            select item;

        var results = await query.ToListAsync();

        return
            from local in localModels
            join result in results on local.Id equals result.Id
            select (local, result);
    }
}

public interface IAssessmentService
{
    Task SaveChanges(IReadOnlyCollection<Assessment> assessments, DeleteLog deleteLog);
}

[Inject(typeof(IAssessmentService))]
public class AssessmentService(ILocalDbCtxFactory factory, ILogger<AssessmentService> logger) : IAssessmentService
{
    public async Task SaveChanges(IReadOnlyCollection<Assessment> assessments, DeleteLog deleteLog)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var updateLog = await db.Assessments.JoinAsync(assessments);

        var addLog = assessments.Where(x => x.IsNew());

        var deleteData = deleteLog.Value().ToList();

        var deleteQuery = db.Assessments
            .Where(x => deleteData.Contains(x.Id));

        foreach (var (localModel, dbModel) in updateLog) dbModel.SetFromAssessmentForm(localModel);

        foreach (var model in addLog) db.Assessments.Add(model);


        LogChanges(db.ChangeTracker, deleteData);
        await deleteQuery.ExecuteDeleteAsync();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private static IEnumerable<LogEntry> GetAddAndUpdateChanges(ChangeTracker tracker)
    {
        return tracker
            .Entries<Assessment>()
            .Select(x => new LogEntry(x.State, x.Entity));
    }

    private static IEnumerable<Assessment> GetAddLog(IReadOnlyCollection<Assessment> assessments)
    {
        var addLog = assessments.Where(x => x.Id == 0);
        return addLog;
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

    private record LogEntry(EntityState State, Assessment Assessment)
    {
    }
}