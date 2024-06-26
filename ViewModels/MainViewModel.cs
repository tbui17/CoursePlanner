using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace ViewModels;

public partial class MainViewModel(IDbContextFactory<LocalDbCtx> factory) : ObservableObject
{
    [ObservableProperty]
    private int _instructorCount;

    [RelayCommand]
    public async Task GetInstructorCountAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var res = await db.Instructors.CountAsync();
        InstructorCount = res;
    }
}