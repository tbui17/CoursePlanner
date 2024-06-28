using FluentAssertions;
using Lib.Utils;


namespace Tests;

public class DatabaseTest : BaseSetup
{
    [Fact]
    public async Task DbSmokeTest()
    {
        await using var db = await Provider.GetLocalDbCtxAsync();

        db
           .Instructors.ToList()
           .Should()
           .HaveCountGreaterThan(2);
    }
}