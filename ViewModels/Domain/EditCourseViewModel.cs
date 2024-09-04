using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

[Inject]
public partial class EditCourseViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService)
    : ObservableObject, ICourseField, IRefreshId
{
    public ObservableCollection<string> Statuses { get; } = Course.Statuses.ToObservableCollection();

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private DateTime _start;

    [ObservableProperty]
    private DateTime _end;

    [ObservableProperty]
    private bool _shouldNotify;

    [ObservableProperty]
    private string _selectedStatus = Course.Statuses.First();

    string ICourseField.Status
    {
        get => SelectedStatus;
        set => SelectedStatus = value;
    }


    [RelayCommand]
    public async Task SaveAsync()
    {
        if (this.ValidateNameAndDates() is { } exc)
        {
            await appService.ShowErrorAsync(exc.Message);
            return;
        }

        await using var db = await factory.CreateDbContextAsync();
        var course = await db
            .Courses
            .AsTracking()
            .FirstAsync(x => x.Id == Id);

        course.SetFromCourseField(this);
        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
    }

    public async Task Init(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        var course = await db
                         .Courses
                         .FirstOrDefaultAsync(x => x.Id == id) ??
                     new();

        this.SetFromCourseField(course);
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}