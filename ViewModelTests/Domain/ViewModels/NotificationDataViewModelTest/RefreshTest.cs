using FluentAssertions;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;
[NonParallelizable]
public class RefreshTest : BasePageViewModelTest
{
    [Test]
    public async Task Refresh_ContinuesToWorkAfterFirstUse()
    {
        var f = Resolve<NotificationDataPaginationTestFixture>();
        f.DataService.Invocations.Clear();
        const int count = 10;

        for (var i = 0; i < count; i++)
        {
            await f.Model.RefreshAsync();
        }

        await f.DataService.Invocations.ShouldEventuallySatisfy(x => x
            .Should()
            .HaveCount(count)
        );

    }
}