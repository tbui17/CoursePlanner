namespace ViewModels.Interfaces;

public interface IMessageDisplay
{
    Task ShowError(string message);
    Task ShowInfo(string message);
}