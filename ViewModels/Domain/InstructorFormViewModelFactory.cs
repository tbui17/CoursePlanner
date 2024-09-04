using Lib.Attributes;
using Lib.Exceptions;
using Lib.Models;
using Lib.Traits;
using Microsoft.EntityFrameworkCore;
using ViewModels.Services;

namespace ViewModels.Domain;

[Inject]
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

                editModel.SetFromContactForm(model);
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