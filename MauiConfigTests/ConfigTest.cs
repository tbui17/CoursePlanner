using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using BaseTestSetup.Lib;
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
using ViewModels.Interfaces;

namespace MauiConfigTests;

public class ConfigTest
{
    private MauiTestFixture _fixture;
    private const string Prefix = "7f1aebf1-8e5a-4f3c-bdb1-00d3aee364bc";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());


        dir.EnumerateDirectories()
            .AsParallel()
            .Where(x => x.Name.StartsWith(Prefix))
            .ForAll(x => x.Delete(true));
    }

    private string _directory;

    [SetUp]
    public void Setup()
    {
        _directory = Prefix + Guid.NewGuid();
        var fixture = CreateFixture();
        var builder = MauiApp.CreateBuilder();
        fixture.Inject(builder);

        var serviceBuilder = fixture.FreezeMock<IMauiServiceBuilder>();
        var registration = fixture.FreezeMock<Action<UnhandledExceptionEventHandler>>();
        var messageDisplay = fixture.FreezeMock<IMessageDisplay>();
        var testPage1 = new TestPage1();
        fixture.Inject(testPage1);

        var fakeExceptionContext = fixture.Freeze<ExceptionContextFake>();
        registration.Setup(x => x(It.IsAny<UnhandledExceptionEventHandler>()))
            .Callback<UnhandledExceptionEventHandler>(x => fakeExceptionContext.Delegates.Add(x));

        var appDataDirectoryGetter = fixture.FreezeMock<Func<string>>();


        appDataDirectoryGetter.Setup(x => x()).Returns(_directory);

        var config = new MauiAppServiceConfiguration
        {
            ServiceBuilder = serviceBuilder.Object,
            Services = builder.Services,
            AppDataDirectory = appDataDirectoryGetter.Object,
            MainPage = messageDisplay.Object,
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
        var fs = new FileInfo(Path.Combine(_directory, "database.db"));
        fs.Exists.Should().BeTrue();

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