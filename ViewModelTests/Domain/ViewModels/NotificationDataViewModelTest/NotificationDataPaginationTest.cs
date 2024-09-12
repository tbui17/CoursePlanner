using FluentAssertions;
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


        f.Model.ChangePageCommand.Execute(2);

        await f.Model.Should()
            .EventuallySatisfy(x => x.PageResult?.CurrentPageData.Should()
                .HaveCount(10)
                .And.AllSatisfy(item => f.DataIds.Should().Contain(item.Id))
            );
    }

    [Test]
    public async Task Pages_PartitionedBy10_GreaterThan1()
    {
        var f = CreateFixture();

        await f.Model.Should()
            .EventuallySatisfy(x => x.PageResult?.PageCount.Should().BeGreaterThan(1));
    }

    [Test]
    public async Task ChangePage_AboveLimit_DefaultsToMax()
    {
        var f = CreateFixture();
        await f.Model.RefreshAsync();

        await f.Model.Should().EventuallySatisfy(x => x.PageResult?.CurrentPageData.Should().HaveCount(10));

        f.Model.ChangePageCommand.Execute(1000);

        await f.Model.Should().EventuallySatisfy(x => x.PageResult?.CurrentPage.Should().Be(5));
    }
}