using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Execution;
using Lib.Attributes;
using Lib.Services;
using Lib.Services.NotificationService;
using Lib.Validators;
using MauiConfig;
using Moq;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.ExceptionHandlers;

namespace MauiConfigTests;

public class ConfigTest
{
    private MauiTestFixture _fixture;


    [SetUp]
    public void Setup()
    {
        var fixture = CreateFixture();
        var builder = MauiApp.CreateBuilder();
        fixture.Inject(builder);

        var serviceBuilder = fixture.FreezeMock<IMauiServiceBuilder>();
        var registration = fixture.FreezeMock<Action<UnhandledExceptionEventHandler>>();
        var mainPageGetter = fixture.FreezeMock<MainPageGetter>();

        var testPage1 = new TestPage1();
        mainPageGetter.Setup(x => x()).Returns(testPage1);
        fixture.Inject(testPage1);

        var fakeExceptionContext = fixture.Freeze<ExceptionContextFake>();
        registration.Setup(x => x(It.IsAny<UnhandledExceptionEventHandler>()))
            .Callback<UnhandledExceptionEventHandler>(x => fakeExceptionContext.Delegates.Add(x));

        var appDataDirectoryGetter = fixture.FreezeMock<Func<string>>();


        appDataDirectoryGetter.Setup(x => x()).Returns("test");

        var config = new MauiAppServiceConfiguration
        {
            ServiceBuilder = serviceBuilder.Object,
            Services = builder.Services,
            AppDataDirectory = appDataDirectoryGetter.Object,
            MainPage = mainPageGetter.Object,
            ExceptionHandlerRegistration = registration.Object,
        };

        config.AddServices();
        fixture.Inject(config);
        fixture.Inject(config.Services);
        fixture.Register(() => builder.Build());

        _fixture = new MauiTestFixture
        {
            Builder = builder,
            Config = config,
            Fixture = fixture,
        };
    }

    [Test]
    public void ExceptionHandlerRegistration_ShouldOccur()
    {
        _fixture.RunStartupActions();
        var registration = Mock.Get(_fixture.Config.ExceptionHandlerRegistration);
        registration.ShouldCall();
        _fixture.Fixture.Create<ExceptionContextFake>().Delegates.Should().HaveCount(1);
    }

    [Test]
    public void DbSetup_ShouldOccur()
    {
        _fixture.RunStartupActions();
        var mainPageGetter = Mock.Get(_fixture.Config.MainPage);
        mainPageGetter.ShouldCall();
    }

    [Test]
    public void Configuration_ContainsRequiredDependencies()
    {
        _fixture.Builder.Services
            .Select(x => x.ImplementationType)
            .Should()
            .Contain(x => x == typeof(AccountService))
            .And.Contain(x => x == typeof(LoginFieldValidator))
            .And.Contain(x => x == typeof(LoginViewModel))
            .And.Contain(x => x == typeof(NotificationService));
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }


    [Test]
    public void Config_ShouldRetrieveConcreteClassesFromDomainNamespace()
    {
        var services = new ServiceCollection();

        var clientConfig = new ViewModelConfig(services);
        clientConfig.AddServices().AddInjectables();

        services
            .Should()
            .ContainSingle(x => x.ServiceType == typeof(InstructorFormViewModelFactory))
            .And.ContainSingle(x => x.ImplementationType == typeof(EditAssessmentViewModel))
            .And.ContainSingle(x => x.ServiceType == typeof(IEditAssessmentViewModel));
    }

    [Test]
    public void Config_ShouldInjectBackendServices()
    {
        var services = new ServiceCollection();

        new ViewModelConfig(services).AddServices().AddInjectables();
        using var _ = new AssertionScope();
        services
            .Where(x => x.ImplementationType == typeof(LoginFieldValidator))
            .Should()
            .HaveCount(2);

        services.Should()
            .ContainSingle(x => x.ServiceType == typeof(IAccountService));
    }
}

file static class FixtureExtensions
{
    public static Mock<T> FreezeMock<T>(this IFixture fixture) where T : class => fixture.Freeze<Mock<T>>();
    public static Mock<T> CreateMock<T>(this IFixture fixture) where T : class => fixture.Create<Mock<T>>();
}

file static class MockExtensions
{
    public static AndWhichConstraint<GenericCollectionAssertions<IInvocation>, IInvocation> ShouldCall<T>(
        this Mock<T> mock, Expression<Func<T, string>> expr) where T : class
    {
        var expr2 = (ConstantExpression)expr.Body;

        var str = (string)expr2.Value!;

        return mock.Invocations.Should().Contain(x => x.Method.Name == str);
    }

    public static AndConstraint<GenericCollectionAssertions<IInvocation>> ShouldCall<T>(this Mock<T> mock)
        where T : class
    {
        using var scope = new AssertionScope();
        var mockInvocations = mock.Invocations.Where(x => x.Method.DeclaringType?.Name is { } s &&
                                                          (s.StartsWith("Action") || s.StartsWith("Func")));

        return mockInvocations.Should().NotBeEmpty($"{mock} should call a Func or Action.");
    }
}

public delegate string NameSelect<T>(Expression<Func<T, string>> expr);

public record MauiTestFixture
{
    public required IFixture Fixture { get; init; }
    public required MauiAppServiceConfiguration Config { get; init; }
    public required MauiAppBuilder Builder { get; init; }

    public void RunStartupActions()
    {
        var app = Builder.Build();
        Config.RunStartupActions(app);
    }
}

file class TestPage1 : ContentPage;

public class ExceptionContextFake
{
    public List<Delegate> Delegates { get; } = [];
}