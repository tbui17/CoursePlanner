using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Serilog.Core.Enrichers;


namespace ViewModelTests.TestSetup;

public abstract class BaseDbTest : BaseTest
{

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
        var logger = Provider.GetRequiredService<ILogger<BaseDbTest>>();
        await using var db = await factory.CreateDbContextAsync();
        logger.LogInformation("Setting up database with connection string: {ConnectionString}",
            db.Database.GetConnectionString());
        await new DbUtil(db).ResetAndSeedDb();
    }

    [TearDown]
    public override async Task TearDown()
    {
        await base.Setup();
        using var _ = TestContextProvider();
        await ResolveResiliencePipeline("Retry")
            .ExecuteAsync(async token =>
                {
                    var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
                    await using var db = await factory.CreateDbContextAsync(token);
                    await db.Database.EnsureDeletedAsync(token);
                }
            );

    }
}