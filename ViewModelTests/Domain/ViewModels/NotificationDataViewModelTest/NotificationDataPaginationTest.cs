using FluentAssertions;
using FluentAssertions.Execution;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(5000)]
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
    public async Task Pages_PartitionedBy10_GreaterThan1()
    {
        var f = CreateFixture();
        await f.ModelEventuallyHasData();

        await f.Model.Should()
            .EventuallySatisfy(x => x.PageResult.As<PageResult>()
                .TotalPageCount.Should()
                .BeGreaterThan(1)
            );
    }


    [Test]
    public async Task ChangePage_AboveLimit_DefaultsToMax()
    {
        var f = CreateFixture();

        await f.ModelEventuallyHasData();
        f.Model.ChangePageCommand.Execute(1000);

        await f.Model.Should()
            .EventuallySatisfy(x =>
                {
                    var s = x.PageResult.As<PageResult>();
                    s.TotalPageCount.Should().Be(5).And.Be(s.CurrentPage);
                }
            );
    }

    [Test]
    public async Task ChangePage_FilterApplied_PageCountAdjustsForReducedAmount()
    {
        var f = CreateFixture();
        await f.ModelEventuallyHasData();

        f.Model.FilterText = "Notification 10";

        await f.Model.Should()
            .EventuallySatisfy(x =>
                {
                    using var _ = new AssertionScope();
                    var page = x.PageResult;
                    page.PageCount.Should().Be(1);
                    page.ItemCount.Should().Be(1);
                    page.CurrentPageData.Should().ContainSingle();
                }
            );
    }
}