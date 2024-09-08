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
using ViewModelTests.TestSetup;

#pragma warning disable CS8974 // Converting method group to non-delegate type

namespace ViewModelTests;

public class ClientExceptionHandlerTest : BaseTest
{
    private ClientExceptionHandlerTestFixtureData CreateTestFixture()
    {
        var f = CreateFixture();
        var log = new FakeLogger<ClientExceptionHandler>();

        f.Register<ILogger<ClientExceptionHandler>>(() => log);
        f.Register(Resolve<GlobalExceptionHandler>);

        var messageDisplay = f.FreezeFake<IMessageDisplay>();

        var handler = f.Create<ClientExceptionHandler>();


        return new ClientExceptionHandlerTestFixtureData(log, messageDisplay, handler);
    }

    private class TestException1 : Exception;

    private class TestException2 : Exception;








    [Test]
    public void OnUnhandledException_NonErrors_OnlyShowsInformationMessages()
    {
        var (log, messageDisplay, handler) = CreateTestFixture();


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
    public void OnUnhandledException_Errors_ShowsErrorMessagesBelowCritical()
    {
        var (log, messageDisplay, handler) = CreateTestFixture();


        var exceptions = new List<Exception>
        {
            new ArgumentException(),
            new NullReferenceException()
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

        messageDisplay.RecordedCalls
            .Where(x => x.Method.Name is nameof(messageDisplay.FakedObject.ShowError))
            .Should()
            .NotBeEmpty();

        log.Collector
            .GetSnapshot()
            .Should()
            .OnlyContain(x => x.Level > LogLevel.Information && x.Level < LogLevel.Critical);
    }

    [Test]
    public async Task OnUnhandledException_Critical_LogsErrorAndThrows()
    {
        var (log, messageDisplay, handler) = CreateTestFixture();

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
            .CompleteWithinAsync(TimeSpan.FromDays(2));

        log.Collector
            .GetSnapshot()
            .Should()
            .Contain(x => x.Level == LogLevel.Critical && x.Exception is TestException1);
    }
}

public record ClientExceptionHandlerTestFixtureData(
    FakeLogger<ClientExceptionHandler> Logger,
    Fake<IMessageDisplay> MessageDisplay,
    ClientExceptionHandler Handler);