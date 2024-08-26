using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Interfaces;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public partial class EditTermViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService)
    : ObservableObject, IDateTimeEntity, IRefresh
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private DateTime _start;

    [ObservableProperty]
    private DateTime _end;

    [RelayCommand]
    public async Task SaveAsync()
    {

        if (this.ValidateNameAndDates() is { } exc)
        {
            await appService.ShowErrorAsync(exc.Message);
            return;
        }

        var db = await factory.CreateDbContextAsync();
        var term = await db
           .Terms
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        term.SetFromDateTimeEntity(this);

        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }

    public async Task Init(int termId)
    {

        await using var db = await factory.CreateDbContextAsync();

        var term = await db
               .Terms
               .FirstOrDefaultAsync(x => x.Id == termId) ??
            new();

        this.SetFromDateTimeEntity(term);
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}