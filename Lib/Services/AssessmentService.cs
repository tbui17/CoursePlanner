using System.Data;
using Lib.Attributes;
using Lib.Models;
using Lib.Traits;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Lib.Services;

public interface IAssessmentService
{
    Task SaveChanges(IReadOnlyCollection<Assessment> assessments, DeleteLogCollection deleteLogCollection);
}

[Inject(typeof(IAssessmentService))]
public class AssessmentService(ILocalDbCtxFactory factory, ILogger<AssessmentService> logger) : IAssessmentService
{

    private static async Task<IEnumerable<(Assessment dbModel, Assessment localModel)>> GetUpdateLog(
        IReadOnlyCollection<Assessment> assessments,
        LocalDbCtx db
    )
    {

        var idsOfItemsToUpdate = assessments.Select(x => x.Id).ToList();

        var toUpdateData = await db
            .Assessments
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
        return tracker
            .Entries<Assessment>()
            .Select(x => new LogEntry(x.State, x.Entity));
    }

    private static IEnumerable<Assessment> GetAddLog(IReadOnlyCollection<Assessment> assessments)
    {
        var addLog = assessments.Where(x => x.Id == 0);
        return addLog;
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
    }


    public async Task SaveChanges(IReadOnlyCollection<Assessment> assessments, DeleteLogCollection deleteLogCollection)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var updateLog = await GetUpdateLog(assessments, db);

        var addLog = GetAddLog(assessments);

        var deleteLog = deleteLogCollection.Value().ToList();

        var deleteQuery = db.Assessments
            .Where(x => deleteLog.Contains(x.Id));

        foreach (var (dbModel, localModel) in updateLog)
        {
            dbModel.SetFromAssessmentForm(localModel);
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


}