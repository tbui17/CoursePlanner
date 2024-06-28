using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner.ViewModels;

public partial class EditTermViewModel(ILocalDbCtxFactory factory, AppShellViewModel appShell)
    : ObservableObject, IQueryAttributable
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
        await appShell.GoBackToDetailedTermPageAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Id = query.Get<int>("Id");
        Name = query.Get<string>("Name");
        Start = query.Get<DateTime>("Start");
        End = query.Get<DateTime>("End");
    }
}