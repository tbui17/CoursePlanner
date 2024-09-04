using Lib.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ViewModels.Setup;


[Inject]
public class SetupClient(DbSetup dbSetup, ILogger<SetupClient> logger)
{
    public void Setup()
    {
        logger.LogInformation("Beginning initialization.");
        dbSetup.SetupDb();
        logger.LogInformation("Initialization complete.");
    }
}

[Inject]
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
            logger.LogInformation("A database entity already exists. Attempting to re-initialize database. {Message}",e.Message);
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            logger.LogInformation("Database re-initialized.");
        }
    }
}