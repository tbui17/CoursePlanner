﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

public interface IEditNoteViewModel
{
    string Name { get; set; }
    string Text { get; set; }
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand BackCommand { get; }
    Task Init(int noteId);
    Task RefreshAsync();
}

[Inject]
public partial class EditNoteViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService)
    : ObservableObject, INoteField, IRefreshId, IEditNoteViewModel
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _text = "";

    string INoteField.Value
    {
        get => Text;
        set => Text = value;
    }

    public async Task Init(int noteId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var note = await db
                       .Notes
                       .FirstOrDefaultAsync(x => x.Id == noteId) ??
                   new();

        this.Assign(note);
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

        await using var db = await factory.CreateDbContextAsync();
        var note = await db
            .Notes
            .AsTracking()
            .FirstAsync(x => x.Id == Id);

        note.Assign(this);


        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }
}