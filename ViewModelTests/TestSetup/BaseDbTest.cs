using Lib.Utils;

namespace ViewModelTests.TestSetup;

public abstract class BaseDbTest
{
    [SetUp]
    public virtual async Task Setup()
    {
        var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
        await using var db = await factory.CreateDbContextAsync();
        await new DbUtil(db).ResetAndSeedDb();
    }
}