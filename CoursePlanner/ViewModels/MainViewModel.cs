using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoursePlanner.Services;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;


namespace CoursePlanner.ViewModels;

public partial class MainViewModel(IDbContextFactory<LocalDbCtx> factory, AppService appShell) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Term> _terms = [];

    [ObservableProperty]
    private Term? _selectedTerm;

    public async Task Init()
    {
        await using var db = await factory.CreateDbContextAsync();
        Terms = await db
           .Terms
           .Include(x => x.Courses)
           .ToListAsync()
           .ToObservableCollectionAsync();
    }

    [RelayCommand]
    public async Task AddTermAsync()
    {
        var name = await appShell.DisplayNamePromptAsync();
        if (name is null)
        {
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        db.Terms.Add(new Term { Name = name });
        await db.SaveChangesAsync();
        await Init();
    }

    [RelayCommand]
    public async Task DetailedTermAsync()
    {
        if (SelectedTerm is null)
        {
            return;
        }

        await appShell.GoToDetailedTermPageAsync(SelectedTerm.Id);
    }

    public bool CanExecuteTermCommand() => SelectedTerm is not null;

    [RelayCommand]
    public async Task DeleteTermAsync()
    {
        if (SelectedTerm is null)
        {
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        await db
           .Terms
           .Where(x => x.Id == SelectedTerm.Id)
           .ExecuteDeleteAsync();
        await Init();
    }
}