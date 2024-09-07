using AutoFixture;
using BaseTestSetup.Lib;
using EntityFramework.Exceptions.Common;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.ExceptionHandlers;
using Lib.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ViewModels.ExceptionHandlers;
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

        f.Register<ILogger<ClientExceptionHandler>>(() => clientLog);
        f.Register<ILogger<GlobalExceptionHandler>>(() => globalLog);
        f.Register(Resolve<GlobalExceptionHandler>);

        var messageDisplay = f.FreezeFake<IMessageDisplay>();

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


        messageDisplay.RecordedCalls
            .Where(x => x.Method.Name is nameof(messageDisplay.FakedObject.ShowInfo))
            .Should()
            .NotBeEmpty();

        messageDisplay.RecordedCalls
            .Where(x => x.Method.Name is nameof(messageDisplay.FakedObject.ShowError))
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


        messageDisplay.RecordedCalls
            .Where(x => x.Method.Name is nameof(messageDisplay.FakedObject.ShowInfo))
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

        messageDisplay.RecordedCalls
            .Where(x => x.Method.Name is nameof(messageDisplay.FakedObject.ShowError))
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
    public async Task OnUnhandledException_Critical_DoesNotThrow()
    {
        var fixture = CreateTestFixture();
        var messageDisplay = fixture.MessageDisplay;
        var handler = fixture.Handler;

        messageDisplay.CallsTo(x => x.ShowError(A<string>._)).Throws(new TestException1());

        var exceptions = new List<Exception>
        {
            new TestException2(),
            new TestException2()
        };


        var act = FluentActions.Awaiting(async () =>
        {
            foreach (var args in exceptions.Select(exc => new UnhandledExceptionEventArgs(exc, false)))
            {
                await handler.OnUnhandledException(args);
            }
        });

        using var scope = new AssertionScope();

        await act
            .Should()
            .NotThrowAsync();
    }

    [Test]
    public async Task OnUnhandledException_Critical_LogsCritical()
    {
        var fixture = CreateTestFixture();
        var log = fixture.Logger;
        var messageDisplay = fixture.MessageDisplay;
        var handler = fixture.Handler;

        messageDisplay.CallsTo(x => x.ShowError(A<string>._)).Throws(new TestException1());

        var exceptions = new List<Exception>
        {
            new TestException2(),
            new TestException2()
        };


        foreach (var args in exceptions.Select(exc => new UnhandledExceptionEventArgs(exc, false)))
        {
            await handler.OnUnhandledException(args);
        }

        log.Collector
            .GetSnapshot()
            .Should()
            .Contain(x => x.Level == LogLevel.Critical && x.Exception is TestException1);
    }

}