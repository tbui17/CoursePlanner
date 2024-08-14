using FluentAssertions;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.PageViewModels;

namespace ViewModelTests;

public class EditTermViewModelTest : BaseDbTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Db = GetDb();
        Model = Resolve<EditTermViewModel>();
    }

    private EditTermViewModel Model { get; set; }

    [TearDown]
    public void TearDown()
    {
        Db.Dispose();
    }

    private LocalDbCtx Db { get; set; }

    [Test]
    public async Task Init_ShouldMapToDbValues()
    {

        var expected = await Db.Terms.FirstAsync();

        Model
           .Should()
           .NotBeNull();
        await Model.Init(1);
        Model.Should().BeEquivalentTo(expected, x => x.ExcludingMissingMembers());
    }

    [Test]
    public async Task SaveAsync_PersistsChangesToDb()
    {
        await Model.Init(1);
        Model.Name = "Test 123";
        Model.Start = DateTime.Now;
        Model.End = DateTime.Now.AddDays(1);
        await Model.SaveAsync();
        var term = await Db.Terms.Where(x => x.Name == "Test 123").ToListAsync();
        term.Should().NotBeEmpty();

    }
}