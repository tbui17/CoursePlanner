using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Utils;
using Microsoft.Extensions.DependencyInjection;
using ViewModels;

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

    [Fact]
    public async Task MainViewModelDbSmokeTest()
    {
        var model = Provider.GetRequiredService<MainViewModel>();
        using var _ = new AssertionScope();
        await model.GetInstructorCountAsync();
        model
           .InstructorCount
           .Should()
           .BeGreaterThan(2);
        await model.GetInstructorCountAsync();
        model
           .InstructorCount
           .Should()
           .BeGreaterThan(2);
    }
}