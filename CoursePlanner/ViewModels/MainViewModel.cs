using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;


namespace CoursePlanner.ViewModels;

public partial class MainViewModel(IDbContextFactory<LocalDbCtx> factory, AppShellViewModel appShell) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Term> _terms = [];

    [ObservableProperty]
    private Term? _selectedTerm;


    public async Task GetTermsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        Terms = await db
           .Terms.Include(x => x.Courses)
           .ToListAsync()
           .ToObservableCollectionAsync();
    }

    public async Task Init()
    {
        await GetTermsAsync();
    }

    public event Action<MainViewModel>? ShowWindowRequested;

    [RelayCommand]
    public async Task AddTermAsync()
    {
        var name = await AppShell.DisplayNamePromptAsync();
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

    [RelayCommand]
    public void ShowWindow()
    {
        ShowWindowRequested?.Invoke(this);
    }
    
    
}