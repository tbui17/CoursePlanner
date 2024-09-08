using AutoFixture;
using BaseTestSetup.Lib;
using EntityFramework.Exceptions.Common;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.ExceptionHandlers;
using Lib.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using ViewModels.ExceptionHandlers;
using ViewModels.Exceptions;
using ViewModels.Interfaces;
using ViewModelTests.TestData;
using ViewModelTests.TestSetup;

namespace ViewModelTests;

public class ClientExceptionHandlerTest : BaseTest
{
    private ClientExceptionHandlerTestFixtureData CreateTestFixture()
    {
        var f = CreateFixture();
        var collector = new FakeLogCollector();
        var clientLog = new FakeLogger<ClientExceptionHandler>(collector);
        var globalLog = new FakeLogger<GlobalExceptionHandler>(collector);


        f.Inject<ILogger<ClientExceptionHandler>>(clientLog);
        f.Inject<ILogger<GlobalExceptionHandler>>(globalLog);
        var messageDisplay = f.FreezeMock<IMessageDisplay>();

        var handler = f.Create<ClientExceptionHandler>();


        return new ClientExceptionHandlerTestFixtureData(clientLog, messageDisplay, handler);
    }

    private class TestException1 : Exception;

    private class TestException2 : Exception;


    [Test]
    public void OnUnhandledException_NonErrors_OnlyShowsInformationMessages()
    {
        var fixture = CreateTestFixture();
        var log = fixture.Logger;
        var messageDisplay = fixture.MessageDisplay;
        var handler = fixture.Handler;


        var exceptions = new List<Exception>
        {
            new UniqueConstraintException(),
            new CannotInsertNullException(),
            new MaxLengthExceededException(),
            new NumericOverflowException(),
            new ReferenceConstraintException(),
            new DomainException(""),
        };

        foreach (var args in exceptions.Select(exc => new UnhandledExceptionEventArgs(exc, false)))
        {
            handler.OnUnhandledException(args).Wait();
        }


        using var scope = new AssertionScope();


        messageDisplay.Invocations
            .Where(x => x.Method.Name is nameof(messageDisplay.Object.ShowInfo))
            .Should()
            .NotBeEmpty();

        messageDisplay.Invocations
            .Where(x => x.Method.Name is nameof(messageDisplay.Object.ShowError))
            .Should()
            .BeEmpty();

        log.Collector
            .GetSnapshot()
            .Should()
            .OnlyContain(x => x.Level <= LogLevel.Information);
    }

    [Test]
    public void OnUnhandledException_Errors_DoesNotShowInformationalMessage()
    {
        var fixture = CreateTestFixture();
        var messageDisplay = fixture.MessageDisplay;
        var handler = fixture.Handler;


        var exceptions = new List<Exception>
        {
            new TestException2(),
            new TestException2()
        };

        foreach (var args in exceptions.Select(exc => new UnhandledExceptionEventArgs(exc, false)))
        {
            handler.OnUnhandledException(args).Wait();
        }


        using var scope = new AssertionScope();


        messageDisplay.Invocations
            .Where(x => x.Method.Name is nameof(messageDisplay.Object.ShowInfo))
            .Should()
            .BeEmpty();
    }


    [Test]
    public void OnUnhandledException_Errors_ShowsClientError()
    {
        var fixture = CreateTestFixture();
        var messageDisplay = fixture.MessageDisplay;
        var handler = fixture.Handler;


        var exceptions = new List<Exception>
        {
            new TestException2(),
            new TestException2()
        };

        foreach (var args in exceptions.Select(exc => new UnhandledExceptionEventArgs(exc, false)))
        {
            handler.OnUnhandledException(args).Wait();
        }


        using var scope = new AssertionScope();

        messageDisplay.Invocations
            .Where(x => x.Method.Name is nameof(messageDisplay.Object.ShowError))
            .Should()
            .NotBeEmpty();
    }

    [Test]
    public void OnUnhandledException_Errors_LogsErrorMessagesBelowCritical()
    {
        var fixture = CreateTestFixture();
        var log = fixture.Logger;
        var handler = fixture.Handler;


        var exceptions = new List<Exception>
        {
            new TestException2(),
            new TestException2()
        };

        foreach (var args in exceptions.Select(exc => new UnhandledExceptionEventArgs(exc, false)))
        {
            handler.OnUnhandledException(args).Wait();
        }


        using var scope = new AssertionScope();


        log.Collector
            .GetSnapshot()
            .Should()
            .OnlyContain(x => x.Level > LogLevel.Information && x.Level < LogLevel.Critical);
    }


    [Test]
    public async Task OnUnhandledException_CriticalExceptShowMessageError_DoesNotThrow()
    {
        var fixture = CreateTestFixture();
        var handler = fixture.Handler;

        var act = () => handler
            .OnUnhandledException(new UnhandledExceptionEventArgs(new TestException1(), false));

        await act.Awaiting(x => x())
            .Should()
            .NotThrowAsync();
    }

    [Test]
    public async Task OnUnhandledException_CriticalShowMessageError_Throws()
    {
        var fixture = CreateTestFixture();
        var messageDisplay = fixture.MessageDisplay;
        var handler = fixture.Handler;

        messageDisplay
            .Setup(x => x.ShowInfo(It.IsAny<string>()))
            .ThrowsAsync(new TestException1());

        messageDisplay
            .Setup(x => x.ShowError(It.IsAny<string>()))
            .ThrowsAsync(new TestException2());

        var act = () => handler
            .OnUnhandledException(new UnhandledExceptionEventArgs(new DomainException(""), false));

        await act.Awaiting(x => x())
            .Should()
            .ThrowAsync<ClientExceptionHandlerException>();
    }

    [Test]
    public async Task OnUnhandledException_Critical_LogsCritical()
    {
        var fixture = CreateTestFixture();
        var log = fixture.Logger;
        var messageDisplay = fixture.MessageDisplay;
        var handler = fixture.Handler;

        messageDisplay
            .Setup(x => x.ShowError(It.IsAny<string>()))
            .ThrowsAsync(new TestException1());

        var act = () => handler.OnUnhandledException(new UnhandledExceptionEventArgs(new TestException1(), false));

        await act.Awaiting(x => x())
            .Should()
            .ThrowAsync<Exception>();

        log.Collector
            .GetSnapshot()
            .Should()
            .Contain(x => x.Level == LogLevel.Critical);
    }

    [Test]
    public async Task OnUnhandledException_NullException_LogsError()
    {
        var fixture = CreateTestFixture();
        var log = fixture.Logger;
        var handler = fixture.Handler;


        await handler.OnUnhandledException(new UnhandledExceptionEventArgs(null!, false));

        log.Collector
            .GetSnapshot()
            .Should()
            .Contain(x => x.Level == LogLevel.Error)
            .And.NotContain(x => x.Level == LogLevel.Critical);
    }


    [Test]
    public async Task OnUnhandledException_NullException_ShowsClientErrorMessage()
    {
        var fixture = CreateTestFixture();

        await fixture.Handler.OnUnhandledException(new UnhandledExceptionEventArgs(null!, false));

        fixture.MessageDisplay
            .Verify(x => x.ShowError(It.IsAny<string>()));
    }
}