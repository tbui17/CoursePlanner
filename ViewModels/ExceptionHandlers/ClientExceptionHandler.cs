using Lib.Attributes;
using Lib.ExceptionHandlers;
using Microsoft.Extensions.Logging;

namespace ViewModels.ExceptionHandlers;

[Inject]
public delegate Page? MainPageGetter();

[Inject]
public class ClientExceptionHandler(
    ILogger<ClientExceptionHandler> logger,
    MainPageGetter mainPageGetter,
    GlobalExceptionHandler globalExceptionHandler
)

{
    public async Task OnUnhandledException(UnhandledExceptionEventArgs args)
    {
        if (mainPageGetter() is not { } p)
        {
            logger.LogCritical("Exception:\n{Exception}", args.ExceptionObject.ToString());
            logger.LogCritical("Unexpected null MainPage");
            throw new ApplicationException(args.ExceptionObject.ToString());
        }

        if (args.ExceptionObject is not Exception exception)
        {
            logger.LogCritical("Unexpected exception object type: {Type}, {Args}",
                args.ExceptionObject.GetType().Name, args.ToString());
            await p.DisplayAlert("Unexpected Application Error",
                "There was an error during exception handling. Please restart the application.", "Cancel");
            return;
        }

        try
        {
            var res = await globalExceptionHandler.HandleAsync(exception);
            if (res.UserFriendly)
            {
                await ShowInfo(res.Message);
                return;
            }

            await ShowUnexpectedError(
                "An unexpected error occurred during the application's lifecycle. Please restart the application to ensure data integrity. An error log can be found within the application's folder."
            );
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Unexpected error occurred during exception handling.");
        }

        return;

        async Task ShowUnexpectedError(string message)
        {
            logger.LogError(exception, "Unhandled exception:\n{Exception}", exception.ToString());

            await p.DisplayAlert("Unexpected Application Error", message, "Cancel");
        }

        async Task ShowInfo(string message)
        {
            logger.LogInformation("Domain exception: {Message}", message);
            await p.DisplayAlert("Info", message, "Cancel");
        }
    }
}