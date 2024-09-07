using Lib.Attributes;
using Lib.ExceptionHandlers;
using Microsoft.Extensions.Logging;

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
        Critical = "An unexpected error occurred during the application's lifecycle. Please restart the application to ensure data integrity. An error log can be found within the application's folder.",
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
            logger.LogCritical(e, "Failed to display error message.");
            throw;

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
                logger.LogCritical(ex, "Unhandled exception occurred during secondary attempt to show display error message");
                throw;
            }
        }
    }

    private async Task Handle(UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is not Exception exception)
        {
            logger.LogError(
                "Unexpected exception object type: {Type}, {Args}",
                args.ExceptionObject.GetType().Name,
                args.ToString()
            );
            await messageDisplay.ShowError(Message.UnexpectedErrorType).ContinueWith(HandleTask);
            return;
        }

        var res = globalExceptionHandler.Handle(exception);
        if (!res.UserFriendly)
        {
            await messageDisplay.ShowError(Message.Error).ContinueWith(HandleTask);
            return;
        }

        logger.LogInformation("Domain exception: {Message}", res.Message);
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
}

internal class ShowException(Exception e) : Exception(e.Message, e);