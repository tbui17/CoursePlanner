using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoursePlanner.Services;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner.ViewModels;

public partial class EditNoteViewModel(ILocalDbCtxFactory factory, INavigationService navService)
    : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _text = "";

    [RelayCommand]
    public async Task SaveAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var note = await db
           .Notes
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        note.Name = Name;
        note.Value = Text;


        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }

    public async Task Init(int noteId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var note = await db
               .Notes
               .FirstOrDefaultAsync(x => x.Id == noteId) ??
            new();

        Name = note.Name;
        Text = note.Value;
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}