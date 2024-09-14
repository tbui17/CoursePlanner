using Lib.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ViewModels.Config;

public interface ISetupClient
{
    void Setup();
}

[Inject(typeof(ISetupClient))]
public class SetupClient(IDbSetup dbSetup, ILogger<ISetupClient> logger) : ISetupClient
{
    public void Setup()
    {
        logger.LogInformation("Beginning initialization.");
        dbSetup.SetupDb();
        logger.LogInformation("Initialization complete.");
    }
}

public interface IDbSetup
{
    void SetupDb();
}

[Inject(typeof(IDbSetup))]
public class DbSetup(ILocalDbCtxFactory factory, ILogger<IDbSetup> logger) : IDbSetup
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