using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

[Inject]
public partial class EditTermViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService)
    : ObservableObject, IDateTimeEntity, IRefreshId
{
    [ObservableProperty]
    private DateTime _end;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private DateTime _start;

    public async Task Init(int termId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var term = await db
                       .Terms
                       .FirstOrDefaultAsync(x => x.Id == termId) ??
                   new();

        this.Assign(term);
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (this.Validate() is { } exc)
        {
            await appService.ShowErrorAsync(exc.Message);
            return;
        }

        var db = await factory.CreateDbContextAsync();
        var term = await db
            .Terms
            .AsTracking()
            .FirstAsync(x => x.Id == Id);

        term.Assign(this);

        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }
}