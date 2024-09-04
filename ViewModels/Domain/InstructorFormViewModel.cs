using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
using ViewModels.Services;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace ViewModels.Domain;

public interface IInstructorFormViewModel : IContact, IRefreshId
{
    Task SaveAsync();
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand BackCommand { get; }
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}

[Inject(typeof(IInstructorFormViewModel))]
public partial class InstructorFormViewModel(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService)
    : ObservableObject, IInstructorFormViewModel
{
    [ObservableProperty]
    private string _title = "Instructor Form";

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _phone = "";

    public Func<Instructor, Task<DomainException?>> InstructorPersistence { get; set; } =
        _ => Task.FromResult<DomainException?>(null);

    [RelayCommand]
    public async Task SaveAsync()
    {

        var message = await InstructorPersistence(new Instructor().SetFromContact(this));

        if (message is not null)
        {
            await appService.ShowErrorAsync(message.Message);
            return;
        }

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

        var instructor = await db
                             .Instructors
                             .FirstOrDefaultAsync(x => x.Id == id) ??
                         new();

        this.SetFromContact(instructor);
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}