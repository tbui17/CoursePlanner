using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests;

public abstract class BaseDbTest
{
    [SetUp]
    public virtual async Task Setup()
    {
        var factory = Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await new TestDataFactory().SeedDatabase(db);

    }
}