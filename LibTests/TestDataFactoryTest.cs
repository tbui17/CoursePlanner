using FluentAssertions;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests;

public class TestDataFactoryTest : BaseTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        var factory = Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>();
        await using var db = await factory.CreateDbContextAsync();
        await new DbUtil(db).ResetAndSeedDb();
    }


    [Test]
    public async Task C6Data()
    {
        var factory = Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>();

        await using var db = await factory.CreateDbContextAsync();

        var query = db.Terms
            .Where(x => x.Name == "Test Term 1")
            .Where(x => x.Courses.Any(y => y.Instructor != null && y.Instructor.Name.ToLower().Contains("anika")))
            .Include(x => x.Courses)
            .ThenInclude(x => x.Assessments)
            .Include(x => x.Courses)
            .ThenInclude(x => x.Instructor);

        await query.Awaiting(x => x.FirstAsync()).Should().NotThrowAsync();
    }
}