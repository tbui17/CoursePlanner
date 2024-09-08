using CommunityToolkit.Mvvm.Messaging;
using Lib.Attributes;
using Lib.ExceptionHandlers;
using Lib.Exceptions;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using OneOf;
using ViewModels.Interfaces;

namespace ViewModels.ExceptionHandlers;

internal record ClientExceptionUserErrorMessage(
    string Critical = "",
    string Error = ""
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
        Error =
            "An unexpected exception occurred. Application service may be degraded. It is advised to restart the application.",
    };


    public async Task OnUnhandledException(UnhandledExceptionEventArgs args)
    {
        await HandleTopLevel(args);
    }


    private async Task HandleTopLevel(UnhandledExceptionEventArgs args)
    {
        var exception = await Handle(args)
            .Match(
                argumentOutOfRangeException =>
                {
                    logger.LogError(argumentOutOfRangeException, "Argument exception occurred during exception handling.");
                    WeakReferenceMessenger.Default.Send(argumentOutOfRangeException);
                    return messageDisplay.ShowError(Message.Error).ToExceptionAsync();
                },
                domainException =>
                {
                    logger.LogInformation(domainException, "User Exception.");
                    WeakReferenceMessenger.Default.Send(domainException);
                    return messageDisplay.ShowInfo(domainException.Message).ToExceptionAsync();
                },
                globalExceptionHandlerResult =>
                {
                    logger.LogError(globalExceptionHandlerResult.Exception, "Unhandled exception: {Message}",
                        globalExceptionHandlerResult.Message);
                    WeakReferenceMessenger.Default.Send(globalExceptionHandlerResult);
                    return messageDisplay.ShowError(Message.Critical).ToExceptionAsync();
                }
            );


        if (exception is not null)
        {
            logger.LogCritical(exception, "Failed to display error message.");
            throw exception;
        }
    }

    private OneOf<ArgumentOutOfRangeException, DomainException, GlobalExceptionHandlerResult> Handle(
        UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is not Exception exception)
        {
            return new ArgumentOutOfRangeException(
                nameof(args),
                args.ExceptionObject,
                "Unexpected exception object type"
            );
        }

        var res = globalExceptionHandler.Handle(exception);
        if (!res.UserFriendly)
        {
            return res;
        }

        return new DomainException(res.Message, res.Exception);
    }
}