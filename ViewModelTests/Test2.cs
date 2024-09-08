using AutoFixture;
using BaseTestSetup.Lib;
using Lib.ExceptionHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using ViewModels.ExceptionHandlers;
using ViewModels.Interfaces;

namespace ViewModelTests;



public class Test2
{

    [Test]
    public void Run()
    {
        var f = CreateFixture();
        var collector = new FakeLogCollector();
        var clientLog = new FakeLogger<ClientExceptionHandler>(collector);
        var globalLog = new FakeLogger<GlobalExceptionHandler>(collector);


        f.Inject<ILogger<ClientExceptionHandler>>(clientLog);
        f.Inject<ILogger<GlobalExceptionHandler>>(globalLog);
        var messageDisplay = f.FreezeMock<IMessageDisplay>();

        var handler = f.Create<ClientExceptionHandler>();
        messageDisplay.Verify(x => x.ShowError(It.IsAny<string>()),Times.Never);
    }

}