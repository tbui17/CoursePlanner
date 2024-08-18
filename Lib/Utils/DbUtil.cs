using Lib.Models;

namespace Lib.Utils;

public class DbUtil(LocalDbCtx db)
{


    public async Task SeedDatabase()
    {
        var dataFactory = new TestDataFactory();
        await dataFactory.SeedDatabase(db);
    }

    public async Task ResetAndSeedDb()
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await SeedDatabase();
    }
}