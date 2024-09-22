using Lib.Models;

namespace BaseTestSetup;

public interface IBaseTest
{
    T Resolve<T>() where T : notnull;
    Task<LocalDbCtx> GetDb();
}