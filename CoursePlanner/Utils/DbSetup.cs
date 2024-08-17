using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoursePlanner.Utils;

public class DbSetup(ILocalDbCtxFactory factory, ILogger<DbSetup> logger)
{
    public void SetupDb()
    {
        using var db = factory.CreateDbContext();
        logger.LogInformation("Attempting to migrate database.");
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Database migrated.");
        }
        catch (SqliteException e) when (e.Message.Contains("already exists"))
        {
            logger.LogInformation(
                $"A database entity already exists. Attempting to re-initialize database. {e.Message}"
            );
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            logger.LogInformation("Database re-initialized.");
        }
    }
}