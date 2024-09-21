using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibTests;

public abstract class BaseDbTest : BaseTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        using var _ = TestContextProvider();
        var factory = Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>();
        var logger = Provider.GetRequiredService<ILogger<BaseDbTest>>();
        await using var db = await factory.CreateDbContextAsync();
        logger.LogInformation("Setting up database with connection string: {ConnectionString}", db.Database.GetConnectionString());
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
                    var factory = Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>();
                    await using var db = await factory.CreateDbContextAsync(token);
                    await db.Database.EnsureDeletedAsync(token);
                }
            );
    }
}