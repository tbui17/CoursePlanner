namespace ViewModels.Exceptions;

public class ClientExceptionHandlerException(string message, Exception innerException) : Exception(message,innerException);