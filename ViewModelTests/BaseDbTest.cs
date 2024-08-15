using Lib.Utils;

namespace ViewModelTests;

public abstract class BaseDbTest
{
    [SetUp]
    public virtual async Task Setup()
    {
        var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await new TestDataFactory().SeedDatabase(db);
    }
}