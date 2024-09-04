using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using FluentValidation;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Utils;
using Lib.Validators;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
using ViewModels.Services;


namespace ViewModels.Domain;

[Inject]
public partial class SettingsViewModel(
    ILocalDbCtxFactory factory,
    ISessionService session,
    IValidator<IUserSetting> validator,
    IAppService appService) : ObservableObject, IRefresh, IUserSetting
{
    [ObservableProperty]
    private int _notificationRange;

    TimeSpan IUserSetting.NotificationRange
    {
        get => TimeSpan.FromDays(NotificationRange);
        set => NotificationRange = (int)value.TotalDays;
    }


    public async Task RefreshAsync()
    {
        var user = Result.Try(session.GetOrThrowUser);
        if (user.IsFailed)
        {
            NotificationRange = 1;
            return;
        }
        await using var db = await factory.CreateDbContextAsync();
        var settings = await db.UserSettings.Where(x => x.UserId == user.Value.Id).FirstAsync();

        NotificationRange = (int)settings.NotificationRange.TotalDays;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (validator.Check(this) is { IsFailed: true } e)
        {
            await appService.ShowErrorAsync(e.ToErrorString());
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        var user = session.GetOrThrowUser();
        var settings = await db.UserSettings.Where(x => x.UserId == user.Id).FirstAsync();
        settings.NotificationRange = TimeSpan.FromDays(NotificationRange);
        await db.SaveChangesAsync();
        await appService.AlertAsync("Settings saved.");
    }
}