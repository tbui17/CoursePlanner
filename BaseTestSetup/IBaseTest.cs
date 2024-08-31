using Lib.Models;

namespace BaseTestSetup;

public interface IBaseTest
{
    IServiceProvider Provider { get; set; }
    Task Setup();
    Task TearDown();
    T Resolve<T>() where T : notnull;
    Task<LocalDbCtx> GetDb();
}