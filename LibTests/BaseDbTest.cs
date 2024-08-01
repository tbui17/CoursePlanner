using Lib.Models;
using Lib.Utils;

namespace LibTests;

public abstract class BaseDbTest
{
    [SetUp]
    public virtual async Task Setup()
    {
        await using var db = new LocalDbCtx();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await new TestDataFactory().SeedDatabase(db);

    }
}