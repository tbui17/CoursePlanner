namespace ViewModels.ExceptionHandlers;

public interface IMessageDisplay
{
    Task ShowError(string message);
    Task ShowInfo(string message);
}