using Lib.Attributes;
using Lib.ExceptionHandlers;
using Lib.Exceptions;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using OneOf;
using ViewModels.Interfaces;

namespace ViewModels.ExceptionHandlers;

[Inject(typeof(IClientExceptionHandler))]
public class ClientExceptionHandler(
    ILogger<ClientExceptionHandler> logger,
    IMessageDisplay messageDisplay,
    GlobalExceptionHandler globalExceptionHandler
) : IClientExceptionHandler
{



    public async Task OnUnhandledException(UnhandledExceptionEventArgs args)
    {
        await HandleTopLevel(args);
    }


    private const string ErrorMessage =
        "An unexpected exception occurred. Application service may be degraded. It is advised to restart the application.";

    private async Task HandleTopLevel(UnhandledExceptionEventArgs args)
    {

        var exception = await Handle(args)
            .Match(
                argumentOutOfRangeException =>
                {
                    logger.LogError(argumentOutOfRangeException, "Argument exception occurred during exception handling.");
                    return messageDisplay.ShowError(ErrorMessage).ToExceptionAsync();
                },
                domainException =>
                {
                    logger.LogInformation(domainException, "User Exception.");
                    return messageDisplay.ShowInfo(domainException.Message).ToExceptionAsync();
                },
                globalExceptionHandlerResult =>
                {
                    logger.LogError(globalExceptionHandlerResult.Exception, "Unhandled exception: {Message}", globalExceptionHandlerResult.Message);
                    return messageDisplay.ShowError(ErrorMessage).ToExceptionAsync();
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