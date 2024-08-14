using Lib.Models;
using Moq;
using ViewModels.Services;

namespace ViewModelTests;

public abstract class BaseMockPageViewModelTest<T>(Func<BaseMockPageViewModelConstructorContext,T> factory) : BaseDbTest where T : notnull
{
    [SetUp]
    public override async Task Setup()
    {
        AppServiceMock = new Mock<IAppService>();
        NavServiceMock = new Mock<INavigation>();
        var ctx = new BaseMockPageViewModelConstructorContext
        {
            AppService = AppServiceMock.Object,
            NavService = NavServiceMock.Object
        };
        Model = factory(ctx);
        await base.Setup();
        Db = GetDb();
    }

    public Mock<INavigation> NavServiceMock { get; set; }

    public Mock<IAppService> AppServiceMock { get; set; }

    protected T Model { get; set; }

    [TearDown]
    public void TearDown()
    {
        Db.Dispose();
    }



    protected LocalDbCtx Db { get; set; }

}

public record BaseMockPageViewModelConstructorContext
{
    public required INavigation NavService { get; set; }
    public required IAppService AppService { get; set; }
}