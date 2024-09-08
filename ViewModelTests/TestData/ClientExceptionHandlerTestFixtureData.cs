using FakeItEasy;
using Microsoft.Extensions.Logging.Testing;
using ViewModels.ExceptionHandlers;
using ViewModels.Interfaces;

namespace ViewModelTests.TestData;


public record ClientExceptionHandlerTestFixtureData(
    FakeLogger<ClientExceptionHandler> Logger,
    Fake<IMessageDisplay> MessageDisplay,
    ClientExceptionHandler Handler);