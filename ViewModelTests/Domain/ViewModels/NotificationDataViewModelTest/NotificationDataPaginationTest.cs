using FluentAssertions;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(8000)]
public class NotificationDataPaginationTest : BaseTest
{
    private NotificationDataPaginationTestFixture CreateFixture()
    {
        return Resolve<NotificationDataPaginationTestFixture>();
    }

    [Test]
    public async Task NotificationItems_PageChangedBy1_ChangesToNextSet()
    {
        var f = CreateFixture();

        var oneToTen = f.GetIdSubset(0);
        var elevenToTwenty = f.GetIdSubset(1);


        await f.Model.Should()
            .EventuallySatisfy(x => x.PageResult,
                x => x.CurrentPageData.Should()
                    .HaveCount(10)
                    .And.Subject.Select(s => s.Id)
                    .Should()
                    .BeSubsetOf(oneToTen)
            );

        f.Model.ChangePageCommand.Execute(2);

        await f.Model.Should().EventuallySatisfy(_ => { f.Model.CurrentPage.Should().Be(2); });

        await f.Model.Should()
            .EventuallySatisfy(x => x.PageResult,
                x => x.CurrentPageData.Should()
                    .HaveCount(10)
                    .And.Subject.Select(s => s.Id)
                    .Should()
                    .BeSubsetOf(elevenToTwenty)
            );
    }

    [Test]
    public async Task ChangePage_RandomOrder_ConsistentlyRetrievesCorrectSubset()
    {
        var f = CreateFixture();
        await f.ModelEventuallyHasData();

        var range = Enumerable.Range(1, 5).ToList();

        var rnd = new Random();

        for (var i = 0; i < 100; i++)
        {
            var index = rnd.Next(range.Count);
            var stePage = range[index];
            await Step(stePage);
        }

        return;

        async Task Step(int setPage)
        {
            f.Model.ChangePageCommand.Execute(setPage);
            await f.Model.Should()
                .EventuallySatisfy(x => x.PageResult,
                    x => x.CurrentPageData.Should()
                        .HaveCount(10)
                        .And.Subject.Select(s => s.Id)
                        .Should()
                        .BeSubsetOf(f.GetIdSubset(Math.Max(setPage - 1, 0)))
                );
        }
    }

    [Test]
    public async Task Pages_PartitionedBy10_GreaterThan1()
    {
        var f = CreateFixture();
        await f.ModelEventuallyHasData();

        await f.Model.Should()
            .EventuallySatisfy(x => x.PageResult, x => x.PageCount.Should().BeGreaterThan(1));
    }


    [Test]
    public async Task ChangePage_AboveLimit_DefaultsToMax()
    {
        var f = CreateFixture();

        await f.ModelEventuallyHasData();
        f.Model.ChangePageCommand.Execute(1000);

        await f.Model.Should().EventuallySatisfy(x => x.CurrentPage.Should().Be(5));
    }
}