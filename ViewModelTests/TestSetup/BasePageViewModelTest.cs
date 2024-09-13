using Lib.Models;
using Moq;
using ViewModels.Services;

namespace ViewModelTests.TestSetup;

public abstract class BasePageViewModelTest : BaseDbTest
{

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Db = await GetDb();
        NavMock = new();
        AppMock = new();
        DbFactory = Resolve<ILocalDbCtxFactory>();
    }


    [TearDown]
    public override async Task TearDown()
    {
        await base.TearDown();
    }

    protected LocalDbCtx Db { get; set; }
    protected Mock<INavigationService> NavMock { get; set; }
    protected AppServiceMock AppMock { get; set; }
    protected ILocalDbCtxFactory DbFactory { get; set; }


}

public class AppServiceMock : Mock<IAppService>
{

    public void VerifyReceivedError(int times = 1)
    {
        Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Exactly(times));
    }

    public void VerifyReceivedError(int times, string message)
    {
        Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Exactly(times),message);
    }
}