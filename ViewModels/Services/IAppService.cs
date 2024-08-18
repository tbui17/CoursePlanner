namespace ViewModels.Services;

public interface IAppService
{
    Task ShareAsync(ShareTextRequest request);
    Task ShowErrorAsync(string message);
    Task<string?> DisplayNamePromptAsync();
    Task AlertAsync(string message);
}