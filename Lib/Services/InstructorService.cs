using FluentValidation;
using Lib.Attributes;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Lib.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lib.Services;

public interface IInstructorService
{
    Task<DomainException?> Add(IContactForm form);
    Task<DomainException?> Update(IContact form);
}

[Inject]
public class InstructorService(ILocalDbCtxFactory factory, IValidator<IContactForm> formValidator, ILogger<InstructorService> logger) : IInstructorService
{

    public async Task<DomainException?> Add(IContactForm form)
    {
        using var _ = logger.MethodScope();
        var instructor = new Instructor().SetFromContactForm(form);
        if (formValidator.GetError(instructor) is { } e)
        {
            logger.LogInformation("Instructor validation failed: {Message}", e.Message);
            return e;
        }

        await using var db = await factory.CreateDbContextAsync();

        var exc = await ValidateNoDuplicateEmail(db, form.Email);

        if (exc != null)
        {
            return exc;
        }

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync();

        return null;
    }

    public async Task<DomainException?> Update(IContact form)
    {

        using var _ = logger.MethodScope();
        await using var db = await factory.CreateDbContextAsync();
        var instructor = await db.Instructors.FirstOrDefaultAsync(x => x.Id == form.Id);

        if (instructor is null)
        {
            logger.LogInformation("Instructor not found.");
            return new DomainException("Instructor not found.");
        }
        instructor.SetFromContactForm(form);
        if (formValidator.GetError(instructor) is { } e)
        {
            logger.LogInformation("Instructor validation failed: {Message}", e.Message);
            return e;
        }

        if (await ValidateNoDuplicateEmail(db, instructor.Email, instructor.Id) is { } exc)
        {
            logger.LogInformation("Duplicate email: {Email}", instructor.Email);
            return exc;
        }
        await db.SaveChangesAsync();
        return null;
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