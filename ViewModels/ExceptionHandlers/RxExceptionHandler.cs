using System.Diagnostics;
using Lib;
using Lib.Attributes;
using Microsoft.Extensions.Logging;

namespace ViewModels.ExceptionHandlers;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class RxExceptionHandler(ILogger<RxExceptionHandler> logger,IClientExceptionHandler handler) : IObserver<Exception>
{

    private void Handle(Exception exception)
    {
        if (Debugger.IsAttached) Debugger.Break();
        handler.OnUnhandledException(new UnhandledExceptionEventArgs(exception, false));
    }

    public void OnNext(Exception exception)
    {
        Handle(exception);
        using var _ = logger.MethodScope();
        logger.LogTrace("");
    }

    public void OnError(Exception exception)
    {
        Handle(exception);
        using var _ = logger.MethodScope();
        logger.LogTrace("");
    }

    public void OnCompleted()
    {
        using var _ = logger.MethodScope();
        logger.LogTrace("");

    }
}