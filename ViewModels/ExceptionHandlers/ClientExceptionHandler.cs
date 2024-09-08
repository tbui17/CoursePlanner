using System.Diagnostics.CodeAnalysis;
using Lib.Attributes;
using Lib.ExceptionHandlers;
using Microsoft.Extensions.Logging;
using ViewModels.Exceptions;
using ViewModels.Interfaces;

namespace ViewModels.ExceptionHandlers;

internal record ClientExceptionUserErrorMessage(
    string Critical = "",
    string Error = "",
    string UnexpectedErrorType = ""
);

[Inject(typeof(IClientExceptionHandler))]
public class ClientExceptionHandler(
    ILogger<ClientExceptionHandler> logger,
    IMessageDisplay messageDisplay,
    GlobalExceptionHandler globalExceptionHandler
) : IClientExceptionHandler
{
    private static readonly ClientExceptionUserErrorMessage Message = new()
    {
        Critical =
            "A critical unexpected error occurred during the application's lifecycle. Please restart the application to ensure data integrity. An error log can be found within the application's folder.",
        UnexpectedErrorType = "An unexpected exception occurred. Application service may be degraded. It is advised to restart the application.",
    };


    public async Task OnUnhandledException(UnhandledExceptionEventArgs args)
    {
        await HandleTopLevel(args);
    }

    private async Task HandleTopLevel(UnhandledExceptionEventArgs args)
    {
        try
        {
            await Handle(args);
        }
        catch (ShowException e)
        {
            const string message = "Failed to display error message.";
            logger.LogCritical(e, message);
            throw new ClientExceptionHandlerException(message, e);
        }
        catch (Exception e)
        {
            try
            {
                logger.LogCritical(e, "Unhandled exception occurred during exception handling.");
                await messageDisplay.ShowError(Message.Critical);
            }
            catch (Exception ex)
            {
                const string message =
                    "Unhandled exception occurred during secondary attempt to show display error message";
                logger.LogCritical(ex, message);

                throw new ClientExceptionHandlerException(message, ex);
            }
        }
    }

    private async Task Handle(UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is not Exception exception)
        {
            logger.LogError("Unexpected exception object type: {Args}", args);
            await messageDisplay.ShowError(Message.UnexpectedErrorType).ContinueWith(HandleTask);
            return;
        }

        var res = globalExceptionHandler.Handle(exception);
        if (!res.UserFriendly)
        {
            await messageDisplay.ShowError(Message.Error).ContinueWith(HandleTask);
            return;
        }

        await messageDisplay.ShowInfo(res.Message).ContinueWith(HandleTask);
    }

    private static Task HandleTask(Task t)
    {
        if (t.IsFaulted)
        {
            throw new ShowException(t.Exception);
        }

        return t;
    }
    [ExcludeFromCodeCoverage]
    private class ShowException(Exception e) : Exception(e.Message, e);
}

