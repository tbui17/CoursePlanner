using Lib.Utils;
namespace ViewModelTests.TestSetup;


public abstract class BaseDbTest : BaseTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
        await using var db = await factory.CreateDbContextAsync();
        await new DbUtil(db).ResetAndSeedDb();
    }

    [TearDown]
    protected override async Task TearDown()
    {
        await base.TearDown();
        var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.EnsureDeletedAsync();
    }
}