using Lib.Models;

namespace ViewModelTests;

public class BasePageViewModelTest<T> : BaseDbTest where T : notnull
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Db = GetDb();
        Model = Resolve<T>();
    }

    protected T Model { get; set; }

    [TearDown]
    public void TearDown()
    {
        Db.Dispose();
    }

    protected LocalDbCtx Db { get; set; }

}