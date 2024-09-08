
using Microsoft.Extensions.Logging.Testing;
using Moq;
using ViewModels.ExceptionHandlers;
using ViewModels.Interfaces;

namespace ViewModelTests.TestData;


public record ClientExceptionHandlerTestFixtureData(
    FakeLogger<ClientExceptionHandler> Logger,
    Mock<IMessageDisplay> MessageDisplay,
    ClientExceptionHandler Handler);