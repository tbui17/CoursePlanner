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
public class InstructorService(
    ILocalDbCtxFactory factory,
    IValidator<IContactForm> formValidator,
    ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<DomainException?> Add(IContactForm form)
    {
        using var _ = logger.MethodScope();
        // validate hydrated model before saving
        var instructor = new Instructor().SetFromContactForm(form);
        if (formValidator.GetError(instructor) is { } e)
        {
            logger.LogInformation("Instructor validation failed: {Message}", e.Message);
            return e;
        }

        await using var db = await factory.CreateDbContextAsync();

        var exc = await ValidateNoDuplicateEmailCreate(db, form.Email);

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
            logger.LogInformation("Instructor not found. {@Data}", form);
            return new DomainException("Instructor not found.");
        }

        instructor.SetFromContactForm(form);
        if (formValidator.GetError(instructor) is { } e)
        {
            logger.LogInformation("Instructor validation failed: {Message}", e.Message);
            return e;
        }

        if (await ValidateNoDuplicateEmailUpdate(db, instructor.Email, instructor.Id) is { } exc)
        {
            logger.LogInformation("Duplicate email: {Email}", instructor.Email);
            return exc;
        }

        await db.SaveChangesAsync();
        return null;
    }

    private static async Task<DomainException?> ValidateNoDuplicateEmailCreate(LocalDbCtx db, string email)
    {
        email = email.ToLower();
        var baseQuery = db.Instructors.Where(x => x.Email.ToLower() == email);

        return await baseQuery
                .Select(x => x.Email)
                .FirstOrDefaultAsync() switch
            {
                not null => new DomainException("Email already exists."),
                _ => null
            };
    }

    private static async Task<DomainException?> ValidateNoDuplicateEmailUpdate(LocalDbCtx db, string email, int id)
    {
        email = email.ToLower();
        var baseQuery = db.Instructors.Where(x => x.Email.ToLower() == email);


        baseQuery = baseQuery.Where(x => x.Id != id);


        return await baseQuery
                .Select(x => x.Email)
                .FirstOrDefaultAsync() switch
            {
                not null => new DomainException("Email already exists."),
                _ => null
            };
    }
}