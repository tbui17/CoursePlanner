using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Interfaces;
using ViewModels.Services;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace ViewModels.PageViewModels;

public interface IInstructorFormViewModel : IContact, IRefresh
{
    Task SaveAsync();
    Task Init(int id);
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand BackCommand { get; }
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}

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

public class InstructorFormViewModelFactory(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService
)
{
    public IInstructorFormViewModel CreateAddingModel()
    {
        var model = CreateModel();
        SetAdding();

        return model;

        void SetAdding()
        {
            model.Title = "Add Instructor";
            model.InstructorPersistence = async instructor =>
            {
                if (instructor.Validate() is { } e)
                {
                    return e;
                }

                await using var db = await factory.CreateDbContextAsync();

                if (await ValidateNoDuplicateEmail(db, instructor.Email) is { } exc)
                {
                    return exc;
                }

                db.Instructors.Add(instructor);
                await db.SaveChangesAsync();

                return null;
            };
        }
    }

    private InstructorFormViewModel CreateModel()
    {
        return new InstructorFormViewModel(factory, navService, appService);
    }

    public IInstructorFormViewModel CreateEditingModel(int instructorId)
    {
        var model = CreateModel();
        SetEditing();
        return model;

        void SetEditing()
        {
            model.Title = "Edit Instructor";
            model.Id = instructorId;

            model.InstructorPersistence = async instructor =>
            {
                if (instructor.Validate() is { } e)
                {
                    return e;
                }


                await using var db = await factory.CreateDbContextAsync();

                if (await ValidateNoDuplicateEmail(db, instructor.Email, instructorId) is { } exc)
                {
                    return exc;
                }

                var editModel = await db.Instructors.FirstAsync(x => x.Id == instructorId);

                editModel.SetFromContactField(model);
                await db.SaveChangesAsync();
                return null;
            };
        }
    }

    public async Task<IInstructorFormViewModel> CreateInitializedEditingModel(int instructorId)
    {
        var model = CreateEditingModel(instructorId);
        await model.Init(instructorId);
        return model;
    }

    private static async Task<DomainException?> ValidateNoDuplicateEmail(LocalDbCtx db, string email, int? id = null)
    {
        email = email.ToLower();
        var baseQuery = db.Instructors.Where(x => x.Email.ToLower() == email);

        if (id is { } instructorId)
        {
            baseQuery = baseQuery.Where(x => x.Id != instructorId);
        }

        return await baseQuery
                .Select(x => x.Email)
                .FirstOrDefaultAsync() switch
            {
                not null => new DomainException("Email already exists."),
                _ => null
            };
    }
}