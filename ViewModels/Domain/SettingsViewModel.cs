using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

[Inject]
public partial class SettingsViewModel(
    ILocalDbCtxFactory factory,
    ISessionService session,
    IValidator<IUserSetting> validator,
    IAppService appService,
    ILogger<SettingsViewModel> logger) : ObservableObject, IRefresh, IUserSetting
{
    [ObservableProperty]
    private int _notificationRange;


    public async Task RefreshAsync()
    {
        var user = session.User;
        if (user is not { } u)
        {
            NotificationRange = 1;
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        var settings = await db.UserSettings.Where(x => x.UserId == u.Id).FirstAsync();
        logger.LogDebug("Settings: {Settings}", settings);

        NotificationRange = (int)settings.NotificationRange.TotalDays;
    }

    TimeSpan IUserSetting.NotificationRange
    {
        get => TimeSpan.FromDays(NotificationRange);
        set => NotificationRange = (int)value.TotalDays;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (validator.GetError(this) is { } e)
        {
            logger.LogInformation("Validation failed: {Error}", e);
            await appService.ShowErrorAsync(e.Message);
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        var user = session.GetOrThrowUser();
        logger.LogDebug("User: {User}", user);
        var settings = await db.UserSettings.Where(x => x.UserId == user.Id).FirstAsync();
        logger.LogDebug("Settings: {Settings}", settings);
        settings.NotificationRange = TimeSpan.FromDays(NotificationRange);
        await db.SaveChangesAsync();
        await appService.AlertAsync("Settings saved.");
    }
}