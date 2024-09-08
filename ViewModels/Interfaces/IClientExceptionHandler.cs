namespace ViewModels.Interfaces;

public interface IClientExceptionHandler
{
    Task OnUnhandledException(UnhandledExceptionEventArgs args);
}