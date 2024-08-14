using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public partial class InstructorFormViewModel(ILocalDbCtxFactory factory, INavigationService navService, IAppService appService)
    : ObservableObject, IInstructorFormFields
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

    public Func<Instructor, Task<DomainException?>>? InstructorPersistence;

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (InstructorPersistence is null)
        {
            throw new InvalidOperationException($"{nameof(InstructorPersistence)} is not set");
        }

        var message = await InstructorPersistence(new Instructor
            {
                Id = Id,
                Name = Name,
                Email = Email,
                Phone = Phone,
            }
        );

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

        Id = instructor.Id;
        Name = instructor.Name;
        Email = instructor.Email;
        Phone = instructor.Phone;
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }

    public void SetEditing(int instructorId)
    {
        Title = "Edit Instructor";
        Id = instructorId;

        InstructorPersistence = async instructor =>
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

            editModel.Name = instructor.Name;
            editModel.Email = instructor.Email;
            editModel.Phone = instructor.Phone;
            await db.SaveChangesAsync();
            return null;
        };
    }

    public void SetAdding()
    {
        Title = "Add Instructor";
        InstructorPersistence = async instructor =>
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

    private static async Task<DomainException?> ValidateNoDuplicateEmail(LocalDbCtx db, string email, int? id = null)
    {
        email = email.ToLower();
        var baseQuery = db.Instructors.Where(x => x.Email.ToLower() == email);

        if (id is { } instructorId)
        {
            baseQuery = baseQuery.Where(x => x.Id != instructorId);
        }

        var maybeDuplicateEmail = await baseQuery
           .Select(x => x.Email)
           .FirstOrDefaultAsync();

        return maybeDuplicateEmail is not null
            ? new DomainException("Email already exists.")
            : null;
    }
}