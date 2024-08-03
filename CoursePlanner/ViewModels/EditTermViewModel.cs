using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoursePlanner.Services;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner.ViewModels;

public partial class EditTermViewModel(ILocalDbCtxFactory factory, INavigationService navService)
    : ObservableObject
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
        var db = await factory.CreateDbContextAsync();
        var term = await db
           .Terms
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        term.Name = Name;
        term.Start = Start;
        term.End = End;

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

        Id = term.Id;
        Name = term.Name;
        Start = term.Start;
        End = term.End;
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}