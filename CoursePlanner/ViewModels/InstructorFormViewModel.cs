using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoursePlanner.Services;
using Lib.Exceptions;
using Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner.ViewModels;

public partial class InstructorFormViewModel(ILocalDbCtxFactory factory, AppService appShell)
    : ObservableObject
{
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
            await appShell.ShowErrorAsync(message.Message);
            return;
        }
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await appShell.PopAsync();
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

    public void Init(Instructor instructor)
    {
        Id = instructor.Id;
        Name = instructor.Name;
        Email = instructor.Email;
        Phone = instructor.Phone;
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}