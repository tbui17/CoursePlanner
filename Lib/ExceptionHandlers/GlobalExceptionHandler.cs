using System.Diagnostics;
using EntityFramework.Exceptions.Common;
using Lib.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lib.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
{
    public async Task<ExceptionResult> HandleAsync(Exception exc)
    {
        switch (exc)
        {
            case DbUpdateException e and (
                UniqueConstraintException
                or CannotInsertNullException
                or MaxLengthExceededException
                or NumericOverflowException
                or ReferenceConstraintException
                ):
                logger.LogInformation(e, "Database Domain Exception: {Message}", e.Message);
                return ExceptionResult.From(e);
            case DomainException e:
                logger.LogInformation(e, "Domain Exception: {Message}", e.Message);
                return new ExceptionResult(e, e.Message, true);
            default:
                logger.LogError(exc, "Unhandled Exception: {Message}", exc.Message);
                return new ExceptionResult(exc, exc.Message);
        }
    }
}

public record ExceptionResult(Exception? Exception = null, string Message = "", bool UserFriendly = false)
{
    public Exception Exception { get; init; } = Exception ?? new Exception("");

    internal static ExceptionResult From(DbUpdateException exception)
    {
        var message = exception switch
        {
            UniqueConstraintException _ => "An entry with the same key already exists.",
            CannotInsertNullException _ => "The input cannot be empty.",
            MaxLengthExceededException _ => "The input exceeds the maximum length allowed.",
            NumericOverflowException _ => "The input is too large.",
            ReferenceConstraintException _ => "The chosen entry does not exist.",
            _ => throw new UnreachableException($"Unexpected exception type: {exception.GetType()}")
        };
        return new ExceptionResult(exception, message, true);
    }
};