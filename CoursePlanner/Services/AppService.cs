using Microsoft.Extensions.Logging;
using ViewModels.Services;

namespace CoursePlanner.Services;

public class AppService(ILogger<AppService> logger) : IAppService
{
    public async Task ShareAsync(ShareTextRequest request)
    {

        logger.LogInformation("Sharing {Request}", request);
        await Share.RequestAsync(request);
    }


    private static Shell Current => Shell.Current;

    public async Task ShowErrorAsync(string message)
    {
        logger.LogInformation("Showing user error message: {Message}", message);
        await Current.DisplayAlert("Error", message, "OK");
    }


    public async Task<string?> DisplayNamePromptAsync()
    {
        logger.LogInformation("Prompting for name");
        var res = await Current.CurrentPage.DisplayPromptAsync("Enter name", "") switch
        {
            { } name when !string.IsNullOrWhiteSpace(name) => name,
            _ => null
        };

        logger.LogInformation("Name entered: {Name}", res);
        return res;
    }


    public async Task AlertAsync(string message)
    {
        logger.LogInformation("Showing user alert message: {Message}", message);
        await Current.DisplayAlert("Alert", message, "OK");
    }
}