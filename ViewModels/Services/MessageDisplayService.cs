using ViewModels.Interfaces;

namespace ViewModels.Services;

public class MessageDisplayService(Func<Page?> current) : IMessageDisplay
{
    public static MessageDisplayService Create()
    {
        return new MessageDisplayService(() => Application.Current?.MainPage);
    }


    public Task ShowError(string message)
    {
        return current()?.DisplayAlert("Error", message, "OK") ?? Task.CompletedTask;
    }

    public Task ShowInfo(string message)
    {
        return current()?.DisplayAlert("Info", message, "OK") ?? Task.CompletedTask;
    }
}
