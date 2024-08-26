using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
namespace ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Services;

public partial class TermViewModel(ILocalDbCtxFactory factory, INavigationService navService, IAppService appService)
    : ObservableObject, IRefresh
{
    [ObservableProperty]
    private ObservableCollection<Term> _terms = [];

    [ObservableProperty]
    private Term? _selectedTerm;


    public async Task Init()
    {
        await using var db = await factory.CreateDbContextAsync();
        var res = await db
            .Terms
            .Include(x => x.Courses)
            .ToListAsync();
        Terms = res.ToObservableCollection();
    }

    public async Task RefreshAsync()
    {
        await Init();
    }

    public Task Init(int _)
    {
        return Init();
    }

    [RelayCommand]
    public async Task AddTermAsync()
    {
        var name = await appService.DisplayNamePromptAsync();
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
    private async Task DetailedTermAsync()
    {
        if (SelectedTerm is null)
        {
            return;
        }

        await navService.GoToDetailedTermPageAsync(SelectedTerm.Id);
    }


    [RelayCommand]
    private async Task DeleteTermAsync()
    {
        if (SelectedTerm is null)
        {
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        await db
            .Terms.Where(x => x.Id == SelectedTerm.Id)
            .ExecuteDeleteAsync();
        await Init();
    }


}