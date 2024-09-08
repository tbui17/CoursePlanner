namespace ViewModels.ExceptionHandlers;

public interface IClientExceptionHandler
{
    Task OnUnhandledException(UnhandledExceptionEventArgs args);
}