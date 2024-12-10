using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ViewModelTests.TestSetup;

public abstract class BaseDbTest : BaseTest
{
    public override async Task Setup()
    {
        await base.Setup();
        var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
        var logger = Provider.GetRequiredService<ILogger<BaseDbTest>>();
        await using var db = await factory.CreateDbContextAsync();
        logger.LogInformation("Setting up database with connection string: {ConnectionString}",
            db.Database.GetConnectionString()
        );
        await new DbUtil(db).ResetAndSeedDb();
    }


    public override async Task TearDown()
    {
        await base.TearDown();
        using var _ = TestContextProvider();
        var logger = Provider.GetRequiredService<ILogger<BaseDbTest>>();

        await ResolveResiliencePipeline("Retry")
            .ExecuteAsync(async token =>
                {
                    var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
                    await using var db = await factory.CreateDbContextAsync(token);
                    logger.LogInformation("Tearing down database with connection string: {ConnectionString}",
                        db.Database.GetConnectionString()
                    );
                    await db.Database.EnsureDeletedAsync(token);
                }
            );
    }
}